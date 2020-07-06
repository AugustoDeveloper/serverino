using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serverino.Watch.Services;
using Serverino.Watch.Models;

namespace Serverino.Watch
{
    static public class Program
    {
        static public void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static public IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => 
                {
                    builder
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsetings.json", true, true)
                        .AddEnvironmentVariables("SRVRN_");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.AddWatcherHostService(configuration);
                });

        static public IServiceCollection AddWatcherHostService(this IServiceCollection service, IConfiguration configuration)
        {
            var appsFolder = configuration.GetValue<string>("watchFolder", "apps");
            var runtimeHostDirectory = Path.Combine(AppContext.BaseDirectory, appsFolder);
            if (!Directory.Exists(runtimeHostDirectory))
            {
                Directory.CreateDirectory(runtimeHostDirectory);
            }
            
            return service
                .AddScoped<IHostManager<Application>, HostApplicationManager>()
                .AddScoped<IApplicationService>(s => new MemoryApplicationService(runtimeHostDirectory))
                .AddScoped<IHostService, MemoryHostService>()
                .AddHostedService<RuntimeHostingWorker>();
        }
    }
}
