using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Commands;
using Serverino.Watch.Models;
using Serverino.Watch.Services;

namespace Serverino.Watch
{
    public class HostApplicationManager : IHostManager<Application>
    {
        private bool disposed;
        private readonly Queue<IAsyncCommand> queueCommands;
        private readonly ILogger<HostApplicationManager> logger;
        private readonly IHostService service;
        
        public HostApplicationManager(ILogger<HostApplicationManager> logger, IHostService service)
        {
            this.queueCommands = new Queue<IAsyncCommand>();
            this.logger = logger;
            this.service = service;
        }

        private IHostManager<Application> EnqueueCommands(IAsyncCommand command)
        {
            this.queueCommands.Enqueue(command);
            return this;
        }

        public IHostManager<Application> AddHost(Application model)
            => EnqueueCommands(new CreateAppHostCommand(model, this.service, logger));

        public IHostManager<Application> ShutdownHost(Application model)
            => EnqueueCommands(new ShutdownAppHostCommand(model, this.service, logger));

        public IHostManager<Application> UpdateHost(Application model)
            => EnqueueCommands(
                new UpdateAppHostCommand(
                    model, 
                    new CreateAppHostCommand(model, this.service, this.logger),
                    new ShutdownAppHostCommand(model, this.service, this.logger),
                    logger));

        async public Task PersistAsync(CancellationToken token = default)
        {
            var tasks = new List<Task>();
            while(!token.IsCancellationRequested && queueCommands.Count > 0)
            {
                
                if (queueCommands.TryDequeue(out var command))
                {
                    tasks.Add(command.ExecuteAsync(token));
                }
            }

            await Task.WhenAll(tasks);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || this.disposed)
            {
                return;
            }
            
            this.queueCommands.Clear();
            var hosts = this.service.GetAll();
            foreach (var host in hosts)
            {
                host.Dispose();
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}