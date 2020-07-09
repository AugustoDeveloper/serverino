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
    public class CreateAppHostCommand : IAsyncCommand
    {
        private readonly Application application;
        private readonly IHostService service;
        private readonly IApplicationService appService;
        private readonly ILogger logger;
        private string LibraryFilename => $"{this.application.ApplicationPath}/{this.application.Name}.dll";
        
        public CreateAppHostCommand(Application app, IApplicationService appService, IHostService service, ILogger logger = null)
        {
            this.application = app ?? throw new ArgumentNullException(nameof(app));
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.appService = appService ?? throw new ArgumentNullException(nameof(appService));
            this.logger = logger;
        }
        
        async public Task ExecuteAsync(CancellationToken token = default)
        {
            try
            {
                if (!Directory.Exists(this.application.ApplicationPath))
                {
                    throw new DirectoryNotFoundException($"The directory not found: {this.application.ApplicationPath}");
                }
                
                if (!File.Exists(this.application.MainLibraryFilename) ||
                    !File.Exists(this.application.ConfigurationFilename))
                {
                    throw new FileNotFoundException(
                        $"Check if the files (library & configuration) is in {this.application.ApplicationPath}\n- {this.application.MainLibraryFilename}\n- {this.application.ConfigurationFilename}");
                }
                this.logger?.LogDebug($"The subdirectory add as an app {this.application.Name}");

                var host = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(configBuilder => 
                    {
                        var localConfiguration = ConfigureAppConfiguration(configBuilder);
                        var port = localConfiguration.GetValue<int>("port");

                        configBuilder.Configure(ConfigureApp);

                        configBuilder.ConfigureServices(ConfigureServices);
                        configBuilder.UseUrls($"http://+:{port}");
                    })
                    .Build();
            
                await host.StartAsync(token);
                this.service.AddNewHost(this.application, host);
                this.appService.PersistHostedApplications(this.application);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurring on CreateAppHostCommand - ExecuteAsync");
                throw new InvalidCommandExectutionException(nameof(CreateAppHostCommand), this.application, ex);
            }
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
                this.logger.LogInformation($"Request on [{localContext.Connection.LocalPort}] {localContext.Request.Path}");
                await next.Invoke();
            });
        }

        private IConfiguration ConfigureAppConfiguration(IWebHostBuilder builder)
        { 
            var conf = new ConfigurationBuilder()
                    .SetBasePath(this.application.ApplicationPath)
                    .AddJsonFile("AppSettings.json", true, true)
                    .Build();
            builder.UseConfiguration(conf);
            return conf;
        }

        private void ConfigureServices(WebHostBuilderContext context, IServiceCollection svc)
        {
            var libraryAssembly = Assembly.LoadFile(this.application.MainLibraryFilename);

            svc.AddControllers();
            svc.AddMvc()
               .AddApplicationPart(libraryAssembly);
        }
    }
}