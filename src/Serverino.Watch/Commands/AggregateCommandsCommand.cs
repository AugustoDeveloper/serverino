using System;
using System.Threading;
using System.Threading.Tasks;

namespace Serverino.Watch.Commands
{
    public class AggregateCommandsCommand : IAsyncCommand
    {
        private readonly IAsyncCommand[] commands;

        public AggregateCommandsCommand(IAsyncCommand[] comands)
        {
            this.commands = comands ?? throw new ArgumentNullException(nameof(commands));
        }

        public async Task ExecuteAsync(CancellationToken token = default)
        {
            foreach (var command in this.commands)
            {
                await command.ExecuteAsync(token);
            }
        }
    }
}