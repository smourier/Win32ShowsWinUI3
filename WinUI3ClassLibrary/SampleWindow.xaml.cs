using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.XamlTypeInfo;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using WinRT;

namespace WinUI3ClassLibrary
{
    public sealed partial class SampleWindow : Window
    {
        private delegate int DllGetActivationFactory(nint activatableClassId, out nint factory);
        private static Package? _package = null;
        private static readonly ConcurrentDictionary<string, nint> _dllsModules = new();
        private static readonly ConcurrentDictionary<string, DllGetActivationFactory> _activationFactories = new();
        private static readonly ConcurrentDictionary<string, string> _dllsByClassIds = new();

        static SampleWindow()
        {
            // find latest WinUI3 package (todays gives me 1.6)
            // this logic could be changed to choose one in particular
            var mgr = new PackageManager();
            var version = new Version(0, 0);
            var packs = mgr.FindPackagesForUser(string.Empty).Where(p =>
            {
                var dn = p.DisplayName;
                const string name = "WindowsAppRuntime.";
                if (dn.StartsWith(name) && Version.TryParse(dn.AsSpan(name.Length), out var v) && v > version)
                {
                    _package = p;
                }
                return false;
            }).ToArray();

            if (_package?.InstalledPath == null)
                throw new InvalidOperationException("WinUI3 package cannot be found.");

            // load manifest to determine activatable classes
            var manifest = Path.Combine(_package.InstalledPath, "AppxManifest.xml");

            // get all classes for a given dll
            var nsMgr = new XmlNamespaceManager(new NameTable());
            nsMgr.AddNamespace("w", "http://schemas.microsoft.com/appx/manifest/foundation/windows10");
            var doc = XDocument.Load(manifest);
            foreach (var element in doc.XPathSelectElements("w:Package/w:Extensions/w:Extension[@Category='windows.activatableClass.inProcessServer']/w:InProcessServer/w:Path", nsMgr))
            {
                var dllName = element.Value;
                foreach (var cls in element.XPathSelectElements("../w:ActivatableClass[@ActivatableClassId]", nsMgr))
                {
                    _dllsByClassIds.AddOrUpdate(cls.Attribute("ActivatableClassId")!.Value, dllName, (k, o) => throw new InvalidOperationException());
                }
            }

            ActivationFactory.ActivationHandler = ActivationHandler;
        }

        private static unsafe nint ActivationHandler(string runtimeClassId, Guid iid)
        {
            if (!_dllsByClassIds.TryGetValue(runtimeClassId, out var dllName))
                return 0;

            if (!_dllsModules.TryGetValue(runtimeClassId, out var module))
            {
                const uint LOAD_WITH_ALTERED_SEARCH_PATH = 8;
                module = LoadLibraryExW(Path.Combine(_package!.InstalledPath, dllName), 0, LOAD_WITH_ALTERED_SEARCH_PATH);
                if (module == 0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                _dllsModules[runtimeClassId] = module;
            }

            if (!_activationFactories.TryGetValue(runtimeClassId, out var activationFactory))
            {
                var address = GetProcAddress(module, "DllGetActivationFactory");
                if (address == 0)
                    throw new InvalidOperationException();

                activationFactory = Marshal.GetDelegateForFunctionPointer<DllGetActivationFactory>(address);
                _activationFactories[runtimeClassId] = activationFactory;
            }

            MarshalString.Pinnable __runtimeClassId = new(runtimeClassId);
            fixed (void* ___runtimeClassId = __runtimeClassId)
            {
                var hr = activationFactory(MarshalString.GetAbi(ref __runtimeClassId), out var factory);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                hr = Marshal.QueryInterface(factory, ref iid, out var ppv);
                Marshal.Release(factory);
                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                return ppv;
            }
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern nint LoadLibraryExW(string lpLibFileName, nint hFile, uint dwFlags);

        [DllImport("kernel32", SetLastError = true)]
        private static extern nint GetProcAddress(nint hModule, string lpProcName);

        public SampleWindow()
        {
            InitializeComponent();
        }

        static DummyApp _app;
        static readonly ApplicationInitializationCallback _applicationInitializationCallback = Init;

        private static void Init(ApplicationInitializationCallbackParams p)
        {
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static int Hello(nint args, int sizeBytes)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var qc = DispatcherQueueController.CreateOnCurrentThread();
            var q = DispatcherQueue.GetForCurrentThread();
            var ctx = new DispatcherQueueSynchronizationContext(q);
            SynchronizationContext.SetSynchronizationContext(ctx);
            _app = new DummyApp();
            Application.Start(_applicationInitializationCallback);
            var xamlSource = new DesktopWindowXamlSource();

            var window = new SampleWindow();
            window.Activate();

            //Bootstrap.TryInitialize(0x00010010, string.Empty, new Microsoft.Windows.ApplicationModel.DynamicDependency.PackageVersion(1, 6), Bootstrap.InitializeOptions.OnNoMatch_ShowUI | Bootstrap.InitializeOptions.OnPackageIdentity_NOOP, out var hr);
            return 12345678;
        }
    }

    public class DummyApp : Application, IXamlMetadataProvider
    {
        private readonly XamlControlsXamlMetaDataProvider provider = new();
        private readonly IXamlMetadataProvider _myLibProvider;

        public DummyApp()
        {
            // find the generated IXamlMetadataProvider for this lib
            var type = GetType().Assembly.GetTypes().First(t => typeof(IXamlMetadataProvider).IsAssignableFrom(t) && t.GetCustomAttribute<GeneratedCodeAttribute>() != null);
            _myLibProvider = (IXamlMetadataProvider)Activator.CreateInstance(type)!;
        }

        public IXamlType GetXamlType(Type type)
        {
            var ret = provider.GetXamlType(type);
            ret ??= _myLibProvider.GetXamlType(type);
            return ret;
        }

        public IXamlType GetXamlType(string fullName)
        {
            var ret = provider.GetXamlType(fullName);
            ret ??= _myLibProvider.GetXamlType(fullName);
            return ret;
        }

        public XmlnsDefinition[] GetXmlnsDefinitions()
        {
            var ret = provider.GetXmlnsDefinitions();
            ret ??= _myLibProvider.GetXmlnsDefinitions();
            return ret;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Resources.MergedDictionaries.Add(new XamlControlsResources());
            base.OnLaunched(args);
        }
    }
}
