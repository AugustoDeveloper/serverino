using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Models;
using Serverino.Watch.Services;

namespace Serverino.Watch.HostedServices
{
    public class RuntimeHostingWorker : BackgroundService
    {
        private readonly ILogger<RuntimeHostingWorker> logger;
        private readonly IApplicationService service;
        private readonly IHostManager<Application> manager;

        public RuntimeHostingWorker(IApplicationService service, IHostManager<Application> manager, ILogger<RuntimeHostingWorker> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }
        
        async protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Hosting Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(2500, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var notHostedApps = this.service.GetNotHostedApplications();
                    var updatedHostedApps = this.service.GetUpdatedApplications();
                    var removedHostedApps = this.service.GetRemovedApplications();

                    ExecuteActionForeach(notHostedApps, app => this.manager.AddHost(app));
                    ExecuteActionForeach(updatedHostedApps, app => this.manager.UpdateHost(app));
                    ExecuteActionForeach(removedHostedApps, app => this.manager.ShutdownHost(app));

                    await this.manager.PersistAsync(stoppingToken);
                }
                catch(Exception ex)
                {
                    this.logger.LogError(ex, "Some error occurring to up app server");
                }
                
                await Task.Delay(100, stoppingToken);
            }
        }

        private void ExecuteActionForeach(IEnumerable<Application> applications, Action<Application> implementation)
        {
            foreach(var app in applications)
            {
                implementation.Invoke(app);
            }
        }

        public override void Dispose()
        {
            this.manager.Dispose();
            this.service.Dispose();
            
            base.Dispose();
        }
    }
}
