using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Models;
using System.Threading;

namespace Serverino.Watch.Commands
{
    public class UpdateAppHostCommand : IAsyncCommand
    {
        private readonly Application application;
        private readonly IAsyncCommand createAppHostCommand;
        private readonly IAsyncCommand shutdownCommand;
        private readonly ILogger logger;
        
        public UpdateAppHostCommand(Application app, IAsyncCommand createAppHostCommand, IAsyncCommand shutdownCommand, ILogger logger = null)
        {
            this.application = app ?? throw new ArgumentNullException(nameof(app));
            this.createAppHostCommand = createAppHostCommand ?? throw new ArgumentNullException(nameof(createAppHostCommand));
            this.shutdownCommand = shutdownCommand ?? throw new ArgumentNullException(nameof(shutdownCommand));
            this.logger =  logger;
        }
        
        async public Task ExecuteAsync(CancellationToken token = default)
        {
            logger?.LogInformation("Execute restart and re-insertion of application {name}", application.Name);
            await this.shutdownCommand.ExecuteAsync(token);
            await this.createAppHostCommand.ExecuteAsync(token);
        }
    }
}