using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Models;
using Serverino.Watch.Services;

namespace Serverino.Watch.Commands.Factories
{
    public class FactoryAsyncCommand : IFactoryAsyncCommand
    {
        private Dictionary<Type, Func<IAsyncCommand>> registeredCreationCommands;
        private Lazy<Application> lazyApplication;
        private Lazy<IApplicationService> lazyApplicationService;
        private Lazy<IHostService> lazyHostService;
        private Lazy<IAsyncCommand[]> lazyCommands;
        private Lazy<ILogger> lazyLogger;

        public FactoryAsyncCommand()
        {
            this.registeredCreationCommands = new Dictionary<Type, Func<IAsyncCommand>>
            {
                {typeof(CreateAppHostCommand), () => new CreateAppHostCommand(this.lazyApplication.Value, this.lazyApplicationService.Value,
                    this.lazyHostService.Value, this.lazyLogger.Value)},
                {typeof(ShutdownAppHostCommand), () => new ShutdownAppHostCommand(this.lazyApplication.Value, this.lazyApplicationService.Value,
                    this.lazyHostService.Value,this.lazyLogger.Value)},
                {typeof(AggregateCommandsCommand), () => new AggregateCommandsCommand(this.lazyCommands.Value)},
                {typeof(IgnoreApplicationCommand), () => new IgnoreApplicationCommand(this.lazyApplication.Value)}
            };
        }

        private void CleanArguments()
        {
            this.lazyApplication = new Lazy<Application>();
            this.lazyApplicationService = new Lazy<IApplicationService>();
            this.lazyCommands = new Lazy<IAsyncCommand[]>();
            this.lazyHostService = new Lazy<IHostService>();
            this.lazyLogger = new Lazy<ILogger>();
        }

        public IFactoryAsyncCommand With(Application application)
        {
            this.lazyApplication = new Lazy<Application>(() => application);
            return this;
        }

        public IFactoryAsyncCommand With(IApplicationService applicationService)
        {
            this.lazyApplicationService = new Lazy<IApplicationService>(() => applicationService);
            return this;
        }

        public IFactoryAsyncCommand With(IHostService hostService)
        {
            this.lazyHostService = new Lazy<IHostService>(() => hostService);
            return this;
        }
        
        public IFactoryAsyncCommand With(ILogger logger)
        {
            this.lazyLogger = new Lazy<ILogger>(() => logger);
            return this;
        }

        public IFactoryAsyncCommand With(params IAsyncCommand[] commands)
        {
            this.lazyCommands = new Lazy<IAsyncCommand[]>(() => commands);
            return this;
        }

        public IAsyncCommand Create<TCommand>() where TCommand : IAsyncCommand
        {
            try
            {
                return this.registeredCreationCommands[typeof(TCommand)].Invoke();
            }
            finally
            {
                CleanArguments();
            }
        }
    }
}