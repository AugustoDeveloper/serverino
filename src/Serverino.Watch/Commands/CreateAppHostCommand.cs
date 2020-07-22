using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using System;
using System.IO;
using System.Threading;
using Serverino.Watch.Commands.Exceptions;

namespace Serverino.Watch.Commands
{
    public class CreateAppHostCommand : AsyncAppCommandBase
    {
        private readonly IHostService service;
        private readonly IApplicationService appService;
        private string LibraryFilename => $"{this.Application.ApplicationPath}/{this.Application.Name}.dll";
        
        public CreateAppHostCommand(
            Application app, 
            IApplicationService appService, 
            IHostService service, 
            ILogger logger = null) : base(app, logger)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.appService = appService ?? throw new ArgumentNullException(nameof(appService));
        }
        
        async protected override Task ExecuteDomainAsync(CancellationToken token = default)
        {
            if (!Directory.Exists(this.Application.ApplicationPath))
            {
                throw new DirectoryNotFoundException($"The directory not found: {this.Application.ApplicationPath}");
            }
                
            if (!File.Exists(this.Application.MainLibraryFilename) ||
                !File.Exists(this.Application.ConfigurationFilename))
            {
                throw new FileNotFoundException(
                    $"Check if the files (library & configuration) is in {this.Application.ApplicationPath}\n- {this.Application.MainLibraryFilename}\n- {this.Application.ConfigurationFilename}");
            }
            this.Logger?.LogDebug($"The subdirectory add as an app {this.Application.Name}");

            int port = 0;

            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(configBuilder => 
                {
                    var localConfiguration = ConfigureAppConfiguration(configBuilder);
                    port = localConfiguration.GetValue<int>("port");

                    configBuilder.Configure(ConfigureApp);
                    if (port < 1)
                    {
                        throw new InvalidOperationException($"The port {port} is invalid.");
                    }

                    configBuilder.ConfigureServices(ConfigureServices);
                    configBuilder.UseUrls($"http://+:{port}");
                    this.Application.Port = port;
                })
                .Build();
            await host.StartAsync(token);
            this.service.AddNewHost(this.Application, host);
            this.appService.PersistHostedApplications(this.Application);
            this.Logger?.LogInformation($"The application {this.Application.Name} is hosting -> http://+:{port}");
        }

        private void ConfigureApp(WebHostBuilderContext context, IApplicationBuilder app)
        {
            app.UseRouting()
               .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Use(async (localContext, next) =>
            {
                this.Logger.LogInformation($"Request on [{localContext.Connection.LocalPort}] {localContext.Request.Path}");
                await next.Invoke();
            });
        }

        private IConfiguration ConfigureAppConfiguration(IWebHostBuilder builder)
        { 
            var conf = new ConfigurationBuilder()
                    .SetBasePath(this.Application.ApplicationPath)
                    .AddJsonFile("AppSettings.json", true, true)
                    .Build();
            builder.UseConfiguration(conf);
            return conf;
        }

        private void ConfigureServices(WebHostBuilderContext context, IServiceCollection svc)
        {
            var libraryAssembly = Assembly.LoadFile(this.Application.MainLibraryFilename);

            svc.AddControllers();
            svc.AddMvc()
               .AddApplicationPart(libraryAssembly);
        }
    }
}