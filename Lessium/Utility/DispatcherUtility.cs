using Lessium.Classes.Wrappers;
using Lessium.Interfaces;
using System.Windows;

namespace Lessium.Utility
{
    public static class DispatcherUtility
    {
        private static IDispatcher dispatcher = null;

        public static IDispatcher Dispatcher
        {
            get 
            {
                if (dispatcher == null)
                {
                    dispatcher = new DispatcherWrapper(Application.Current.Dispatcher);
                }

                return dispatcher;
            }
            set { dispatcher = value; }
        }
    }
}
