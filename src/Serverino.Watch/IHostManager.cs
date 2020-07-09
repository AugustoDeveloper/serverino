using System;
using System.Threading;
using System.Threading.Tasks;
using Serverino.Watch.Models;

namespace Serverino.Watch
{
    public interface IHostManager<T> : IDisposable
    {
        IHostManager<T> AddHost(T model);
        IHostManager<T> UpdateHost(T model);
        IHostManager<T> ShutdownHost(T model);
        Task PersistAsync(CancellationToken token = default);
    }
}