using Lessium.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Lessium.Classes.Wrappers
{
    public class DispatcherWrapper : IDispatcher
    {
        private Dispatcher dispatcher;

        public DispatcherWrapper(Dispatcher dispatcher)
        {
            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            this.dispatcher = dispatcher;
        }

        public void Invoke(Action action)
        {
            dispatcher.Invoke(action);
        }

        public T Invoke<T>(Func<T> func)
        {
            return dispatcher.Invoke(func);
        }

        public Task InvokeAsync(Action action)
        {
            return dispatcher.InvokeAsync(action).Task;
        }

        public Task<T> InvokeAsync<T>(Func<T> func)
        {
            return dispatcher.InvokeAsync(func).Task;
        }
    }
}
