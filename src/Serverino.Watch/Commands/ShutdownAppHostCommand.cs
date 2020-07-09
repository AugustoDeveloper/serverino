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
    public class ShutdownAppHostCommand : AsyncAppCommandBase
    {
        private readonly IHostService service;
        private readonly IApplicationService appService;

        public ShutdownAppHostCommand(Application app, IApplicationService appService, IHostService service, ILogger logger = null) : base(app, logger)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.appService = appService ?? throw new ArgumentNullException(nameof(appService));
        }
        async protected override Task ExecuteDomainAsync(CancellationToken token = default)
        {
            using var host = this.service.GetByApp(this.Application);
            if (host != null)
            {
                this.Logger?.LogInformation(
                    $"[{this.Application.HostedKey}]Shutdowning the application {this.Application.Name} at {this.Application.ApplicationPath}");
                await host.StopAsync(token);
                this.Logger?.LogInformation(
                    $"[{this.Application.HostedKey}]Application {this.Application.Name} stopped at {DateTime.Now}");
            }
            this.Logger?.LogInformation($"[{this.Application.HostedKey}]Application {this.Application.Name} Removed Host at {DateTime.Now}");
            this.service.RemoveHost(this.Application);
            this.appService.RemoveHostedApplication(this.Application.Name);
            this.Logger?.LogInformation($"[{this.Application.Name}] Removed Application at {DateTime.Now}");
        }
    }
}