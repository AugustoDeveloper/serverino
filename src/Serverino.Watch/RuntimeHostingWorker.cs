using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Serverino.Watch
{
    public class RuntimeHostingWorker : BackgroundService
    {
        private readonly ILogger<RuntimeHostingWorker> logger;
        private readonly string runtimeHostFolder;
        private Dictionary<string, IHost> loadedApps;

        public RuntimeHostingWorker(ILogger<RuntimeHostingWorker> logger, string runtimeHostFolder)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.runtimeHostFolder = runtimeHostFolder ?? throw new ArgumentNullException(nameof(runtimeHostFolder));
            this.loadedApps = new Dictionary<string, IHost>();
        }
        async protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(5000, stoppingToken);
            this.logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var subdirectories = Directory.GetDirectories(this.runtimeHostFolder);

                    var appServers = subdirectories.Select(s => AddSubDirectoryAsAppIfNotLoaded(new DirectoryInfo(s), stoppingToken)).ToArray();
                    await Task.WhenAll(appServers);
                }
                catch(Exception ex)
                {
                    this.logger.LogError(ex, "Some error occurring to up app server");
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        async private Task AddSubDirectoryAsAppIfNotLoaded(DirectoryInfo subDirectory, CancellationToken stoppingToken)
        {
            if (!this.loadedApps.ContainsKey(subDirectory.Name))
            {
                var dllFileName = $"{subDirectory.FullName}//{subDirectory.Name}.dll";
                var dllAssembly = Assembly.LoadFile(dllFileName);

                this.logger.LogInformation($"The subdirectory add as an app {subDirectory.Name}");
                var host = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(configBuilder => 
                    {
                        var localConfiguration = new ConfigurationBuilder()
                            .SetBasePath(subDirectory.FullName)
                            .AddJsonFile("appsettings.json", true, true)
                            .Build();

                        var x = dllAssembly.CreateInstance($"{subDirectory.Name}.Startup");
                        this.logger.LogInformation($"This is a startup instance {x}");

                        var port = localConfiguration.GetValue<int>("port");

                        configBuilder.UseConfiguration(localConfiguration);

                        configBuilder.Configure((wbcontext, app) => 
                        {
                            app.UseRouting();
                            app.UseEndpoints(endp =>
                            {
                                endp.MapControllers();
                            });
                        });

                        configBuilder.ConfigureServices((wbContext, svc) => 
                        {
                            svc.AddControllers();
                            svc.AddMvc(options =>
                            {
                            }).AddApplicationPart(dllAssembly);
                        });
                        configBuilder.UseUrls($"http://+:{port}");
                    })
                    .Build();
                await host.StartAsync(stoppingToken);

                this.loadedApps.TryAdd(subDirectory.Name, host);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach(var host in this.loadedApps.Values)
            {
                host.Dispose();
            }
        }
    }
}
