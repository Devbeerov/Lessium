using Lessium.Classes;
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
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Registers "MainWindow" as key for navigation.
            // But, we don't using navigation yet, so it's not really useful.
            containerRegistry.RegisterForNavigation<MainWindow>();
        }
    }
}
