using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.XamlTypeInfo;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using Windows.Graphics;

namespace WinUI3ClassLibrary
{
    public sealed partial class SampleWindow : Window
    {
        public SampleWindow()
        {
            InitializeComponent();
        }

        private int _dialogResult;

        private void OnCancelButtonClick(object sender, RoutedEventArgs e) => Close();
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            _dialogResult = 1;
            Close();
        }

        static DummyApp? _app;
        public static int ShowWindow(nint args, int sizeBytes)
        {
            // ask for WinAppSDK 1.6
            if (!Bootstrap.TryInitialize(0x00010006, string.Empty, new PackageVersion(), Bootstrap.InitializeOptions.OnNoMatch_ShowUI, out var hr))
                return hr;

            var dialogResult = 0;

            // init an app to get XAML support
            //_app ??= new DummyApp();
            _ = new DummyApp();
            Application.Start(p =>
            {
                // trick to simulate "modality" of our window
                EnableWindow(args, false);

                var window = new SampleWindow();
                window.Activate();

                // resize & center
                var size = 500;
                window.AppWindow.Resize(new SizeInt32(size, size));
                var displayArea = DisplayArea.GetFromWindowId(window.AppWindow.Id, DisplayAreaFallback.Nearest);
                window.AppWindow.Move(new PointInt32((displayArea.WorkArea.Width - size) / 2, (displayArea.WorkArea.Height - size) / 2));

                dialogResult = window._dialogResult;
            });

            EnableWindow(args, true);
            SetActiveWindow(args);
            return dialogResult;
        }

        [DllImport("user32")]
        private static extern bool EnableWindow(nint hWnd, bool bEnable);

        [DllImport("user32")]
        private static extern bool SetActiveWindow(nint hWnd);

        // this is needed for proper XAML support
        private sealed class DummyApp : Application, IXamlMetadataProvider
        {
            private readonly XamlControlsXamlMetaDataProvider provider = new();
            private readonly IXamlMetadataProvider _myLibProvider;
            private static bool _xamlLoaded;

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
                if (!_xamlLoaded)
                {
                    Resources.MergedDictionaries.Add(new XamlControlsResources());
                    _xamlLoaded = true;
                    base.OnLaunched(args);
                }
            }
        }
    }
}
