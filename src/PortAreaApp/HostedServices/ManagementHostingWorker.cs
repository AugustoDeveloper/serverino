using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace PortAreaApp.HostedServices
{
    public class ManagementHostingWorker : BackgroundService
    {
        private readonly IHost managementHost;

        public ManagementHostingWorker()
        {
            this.managementHost = ManagementBuilder.CreateHost(null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => this.managementHost.RunAsync(stoppingToken);

        public override void Dispose()
        {
            this.managementHost.Dispose();
            base.Dispose();
        }
    }
}