using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Commands.Exceptions;
using Serverino.Watch.Models;

namespace Serverino.Watch.Commands
{
    abstract public class AsyncAppCommandBase : IAsyncCommand
    {
        protected Application Application { get; }
        protected ILogger Logger { get; }
        
        protected AsyncAppCommandBase(Application application, ILogger logger)
        {
            this.Application = application ?? throw new ArgumentNullException(nameof(application));;
            this.Logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken token = default)
        {
            try
            {
                await ExecuteDomainAsync(token);
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "An error occurring on CreateAppHostCommand - ExecuteAsync");
                throw new InvalidCommandExectutionException(nameof(CreateAppHostCommand), this.Application, ex);
            }
        }

        abstract protected Task ExecuteDomainAsync(CancellationToken token = default);
    }
}