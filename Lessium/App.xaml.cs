using Lessium.Properties;
using Lessium.Views;
using Prism.Ioc;
using System.Windows;

namespace Lessium
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
            Hotkeys.Current.Save();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Registers MainWindow as key for navigation.

            containerRegistry.RegisterForNavigation<MainWindow>();
        }
    }
}
