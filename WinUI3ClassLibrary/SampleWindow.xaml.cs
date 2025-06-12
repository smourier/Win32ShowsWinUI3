using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.XamlTypeInfo;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace WinUI3ClassLibrary
{
    public partial class SampleWindow : Window
    {
        public SampleWindow()
        {
            InitializeComponent();
        }

        public int DialogResult { get; protected set; }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e) => Close();
        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = 1;
            Close();
        }

        private static DummyApp? _app;

        // this is called by the Win32 app (see hosting.cpp)
#pragma warning disable IDE0060 // Remove unused parameter
        public static int ShowWindow(nint args, int sizeBytes)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            // ask for WinAppSDK 1.6 or 1.7
            if (!Bootstrap.TryInitialize(0x00010007, string.Empty, new PackageVersion(), Bootstrap.InitializeOptions.None, out var hr) &&
                !Bootstrap.TryInitialize(0x00010006, string.Empty, new PackageVersion(), Bootstrap.InitializeOptions.OnNoMatch_ShowUI, out hr))
                return hr;

            if (_app == null)
            {
                // comment this line if you don't want WinUI3 styles
                _app = new DummyApp();
                DispatcherQueueController.CreateOnCurrentThread();
            }

            var _source = new DesktopWindowXamlSource();
            _source.Initialize(Win32Interop.GetWindowIdFromWindow(args));

            var button = new Button()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = "Click me!",
            };

            var grid = new Grid() { Background = new SolidColorBrush(Colors.LightBlue) };
            grid.Children.Add(button);

            button.Click += async (s, e) =>
            {
                var contentDialog = new ContentDialog()
                {
                    XamlRoot = grid.XamlRoot,
                    Title = "Information",
                    Content = "Hello from WinUI 3!",
                    CloseButtonText = "OK"
                };
                await contentDialog.ShowAsync();
                _source.Dispose();
            };

            _source.Content = grid;
            return 0;
        }

        // this is needed for proper XAML support
        private sealed partial class DummyApp : Application, IXamlMetadataProvider
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
}
