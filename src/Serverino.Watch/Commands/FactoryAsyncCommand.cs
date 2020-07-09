using System;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Models;
using Serverino.Watch.Services;

namespace Serverino.Watch.Commands
{
    public class FactoryAsyncCommand : IFactoryAsyncCommand
    {
        private Func<Application> funcApplication;
        private Func<IApplicationService> funcApplicationService;
        private Func<IHostService> funcHostService;
        private Func<IAsyncCommand[]> funcCommands;
        private Func<ILogger> funcLogger;

        public IFactoryAsyncCommand With(Application application)
        {
            this.funcApplication = () => application;
            return this;
        }

        public IFactoryAsyncCommand With(IApplicationService applicationService)
        {
            this.funcApplicationService = () => applicationService;
            return this;
        }

        public IFactoryAsyncCommand With(IHostService hostService)
        {
            this.funcHostService = () => hostService;
            return this;
        }
        
        public IFactoryAsyncCommand With(ILogger logger)
        {
            this.funcLogger = () => logger;
            return this;
        }

        public IFactoryAsyncCommand With(params IAsyncCommand[] commands)
        {
            this.funcCommands = () => commands;
            return this;
        }

        public IAsyncCommand Create<TCommand>() where TCommand : IAsyncCommand
        {
            if (typeof(TCommand) == typeof(CreateAppHostCommand))
            {
                return new CreateAppHostCommand(this.funcApplication?.Invoke(), this.funcApplicationService?.Invoke(),
                    this.funcHostService?.Invoke(), this.funcLogger?.Invoke());
            }
            
            if (typeof(TCommand) == typeof(ShutdownAppHostCommand))
            {
                return new ShutdownAppHostCommand(this.funcApplication?.Invoke(), this.funcApplicationService?.Invoke(),
                    this.funcHostService?.Invoke(),
                    this.funcLogger?.Invoke());
            }

            if (typeof(TCommand) == typeof(AggregateCommandsCommand))
            {
                return new AggregateCommandsCommand(this.funcCommands.Invoke());
            }
            
            if (typeof(TCommand) == typeof(IgnoreApplicationCommand))
            {
                return new IgnoreApplicationCommand(this.funcApplication.Invoke());
            }
            
            throw new InvalidOperationException($"There's no command mapped for this factory {typeof(TCommand).Name}");
        }
    }
}