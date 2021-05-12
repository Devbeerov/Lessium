using System.Collections.Generic;
using System.Windows;

namespace Lessium.Services
{
    public static class UndoableActionsServiceLocator
    {
        private static Dictionary<Window, UndoableActionsService> services = new Dictionary<Window, UndoableActionsService>();

        public static void RegisterService(UndoableActionsService service, Window window)
        {
            services.Add(window, service);

            window.Closed += (sender, e) =>
            {
                UnregisterService(sender as Window);
            };
        }

        public static void UnregisterService(Window window)
        {
            // Service could be already unregistered, so we should check it and return if true.

            if (!services.ContainsKey(window)) return;

            services.Remove(window);
        }

        public static UndoableActionsService GetService(DependencyObject dependencyObject)
        {
            var window = Window.GetWindow(dependencyObject);

            if (window == null) throw new KeyNotFoundException("Failed to find Window parent of argument.");

            return services[window];
        }
    }
}
