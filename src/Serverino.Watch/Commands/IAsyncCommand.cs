using System.Threading;
using System.Threading.Tasks;

namespace Serverino.Watch.Commands
{
    public interface IAsyncCommand
    {
        Task ExecuteAsync(CancellationToken token = default);
    }
}