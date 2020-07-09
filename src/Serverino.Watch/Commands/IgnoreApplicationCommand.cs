using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Models;

namespace Serverino.Watch.Commands
{
    public class IgnoreApplicationCommand : AsyncAppCommandBase
    {
        public IgnoreApplicationCommand(Application app, ILogger logger = null) : base(app, logger) { }

        protected override Task ExecuteDomainAsync(CancellationToken token = default)
        {
            var ignorePath = this.Application.ApplicationPath.Replace(this.Application.Name, ".ignore");
            var ignoreAppPath = Path.Combine(ignorePath, $"{Guid.NewGuid()}-{this.Application.Name}");
            
            if (!Directory.Exists(ignorePath))
            {
                Directory.CreateDirectory(ignorePath);
            }

            Directory.Move(this.Application.ApplicationPath, ignoreAppPath);
            return Task.CompletedTask;
        }
    }
}