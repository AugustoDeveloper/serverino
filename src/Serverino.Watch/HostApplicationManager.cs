using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IApplicationService appService;
        private readonly IFactoryAsyncCommand factoryCommand;

        public HostApplicationManager(IApplicationService appService, IHostService service,
            IFactoryAsyncCommand factoryCommand, ILogger<HostApplicationManager> logger)
        {
            this.queueCommands = new Queue<IAsyncCommand>();
            this.service = service ?? throw new ArgumentNullException(nameof(this.service));
            this.appService = appService ?? throw new ArgumentNullException(nameof(this.appService));
            this.factoryCommand = factoryCommand ?? throw new ArgumentNullException(nameof(this.appService));
            this.logger = logger;
        }

        public IHostManager<Application> AddHost(Application model)
        {
            model = model ?? throw new ArgumentNullException(nameof(model));
            var command = this.factoryCommand
                .With(model)
                .With(this.appService)
                .With(this.service)
                .With(this.logger)
                .Create<CreateAppHostCommand>();
            this.queueCommands.Enqueue(command);
            
            return this;
        } 

        public IHostManager<Application> ShutdownHost(Application model)
        {
            model = model ?? throw new ArgumentNullException(nameof(model));
            var command = this.factoryCommand
                .With(model)
                .With(this.service)
                .With(this.logger)
                .Create<ShutdownAppHostCommand>();
            
            this.queueCommands.Enqueue(command);
            return this;
        }

        public IHostManager<Application> UpdateHost(Application model)
        {
            model = model ?? throw new ArgumentNullException(nameof(model));
            var createAppHostCommand = this.factoryCommand
                .With(model)
                .With(this.appService)
                .With(this.service)
                .With(this.logger)
                .Create<CreateAppHostCommand>();
            
            var shutdownAppHostCommand = this.factoryCommand
                .With(model)
                .With(this.service)
                .With(this.logger)
                .Create<ShutdownAppHostCommand>();

            var aggregateCommands = this.factoryCommand
                .With(shutdownAppHostCommand, createAppHostCommand)
                .Create<AggregateCommandsCommand>();
            
            this.queueCommands.Enqueue(aggregateCommands);
            return this;
        }

        async public Task PersistAsync(CancellationToken token = default)
        {
            var tasks = new List<Task>();
            while(!token.IsCancellationRequested && queueCommands.Any())
            {
                var taskCommand = queueCommands.TryDequeue(out var command)
                    ? command.ExecuteAsync(token)
                    : Task.CompletedTask;
                
                tasks.Add(taskCommand);
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