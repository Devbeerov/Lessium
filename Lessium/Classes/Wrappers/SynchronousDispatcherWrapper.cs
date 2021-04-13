using Lessium.Interfaces;
using System;
using System.Threading.Tasks;

namespace Lessium.Classes.Wrappers
{
    public class SynchronousDispatcherWrapper : IDispatcher
    {
        public void Invoke(Action action)
        {
            action();
        }

        public T Invoke<T>(Func<T> func)
        {
            T result = func();

            return result;
        }

        public Task InvokeAsync(Action action)
        {
            Invoke(action);

            return Task.CompletedTask;
        }

        public Task<T> InvokeAsync<T>(Func<T> func)
        {
            var result = func();

            return Task.FromResult(result);
        }
    }
}
