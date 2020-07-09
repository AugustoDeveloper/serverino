using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Serverino.Watch.Commands.Exceptions;

namespace Serverino.Watch.Commands
{
    public class ShutdownAppHostCommand : IAsyncCommand
    {
        private readonly Application application;
        private readonly IHostService service;
        private readonly ILogger logger;
        private readonly IApplicationService appService;

        public ShutdownAppHostCommand(Application app, IApplicationService appService, IHostService service, ILogger logger = null)
        {
            this.application = app ?? throw new ArgumentNullException(nameof(app));
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.appService = appService ?? throw new ArgumentNullException(nameof(appService));
            this.logger = logger;
        }
        public async Task ExecuteAsync(CancellationToken token = default)
        {
            try
            {
                using var host = this.service.GetByApp(this.application);
                if (host == null)
                {
                    return;
                }
            
                this.logger?.LogInformation($"[{this.application.HostedKey}]Shutdowning the application {this.application.Name} at {this.application.ApplicationPath}");
                await host.StopAsync(token);
                this.service.RemoveHost(this.application);
                this.appService.RemoveHostedApplication(this.application.Name);
                this.logger?.LogInformation($"[{this.application.HostedKey}]Application {this.application.Name} stopped at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                this.logger?.LogError(ex, "An error occuring on ShutdownAppHostCommand - ExecuteAsync");
                throw new InvalidCommandExectutionException(nameof(ShutdownAppHostCommand), this.application, ex);
            }
        }
    }
}