using System;
using System.Threading.Tasks;

namespace Lessium.Interfaces
{
    public interface IDispatcher
    {
        void Invoke(Action action);
        T Invoke<T>(Func<T> func);

        Task InvokeAsync(Action action);
        Task<T> InvokeAsync<T>(Func<T> func);
    }
}
