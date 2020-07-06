using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using System.Threading;

namespace Serverino.Watch.Commands
{
    public class ShutdownAppHostCommand : IAsyncCommand
    {
        private readonly Application application;
        private readonly IHostService service;
        private readonly ILogger logger;
        public ShutdownAppHostCommand(Application app, IHostService service, ILogger logger = null)
        {
            this.application = app ?? throw new ArgumentNullException(nameof(app));
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.logger = logger;
        }
        public async Task ExecuteAsync(CancellationToken token = default)
        {
            using var host = this.service.GetByApp(this.application);
            if (host == null)
            {
                return;
            }
            
            this.logger?.LogDebug($"[{this.application.HostedKey}]Shutdowning the application {this.application.Name} at {this.application.ApplicationPath}");
            await host.StopAsync(token);
            this.service.RemoveHost(this.application);
            this.logger?.LogDebug($"[{this.application.HostedKey}]Application {this.application.Name} stopped at {DateTime.Now}");
        }
    }
}