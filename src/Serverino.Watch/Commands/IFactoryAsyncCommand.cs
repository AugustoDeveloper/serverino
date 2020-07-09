using Microsoft.Extensions.Logging;
using Serverino.Watch.Models;
using Serverino.Watch.Services;

namespace Serverino.Watch.Commands
{
    public interface IFactoryAsyncCommand
    {
        IFactoryAsyncCommand With(Application application);
        IFactoryAsyncCommand With(IApplicationService applicationService);
        IFactoryAsyncCommand With(IHostService hostService);
        IFactoryAsyncCommand With(ILogger logger);
        IFactoryAsyncCommand With(params IAsyncCommand[] commands);
        IAsyncCommand Create<TCommand>() where TCommand : IAsyncCommand;
    }
}