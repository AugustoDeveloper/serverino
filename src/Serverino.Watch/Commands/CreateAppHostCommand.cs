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
using System.Threading;

namespace Serverino.Watch.Commands
{
    public class CreateAppHostCommand : IAsyncCommand
    {
        private readonly Application application;
        private readonly IHostService service;
        private readonly ILogger logger;
        private string LibraryFilename => $"{this.application.ApplicationPath}//{this.application.Name}.dll";
        
        public CreateAppHostCommand(Application app, IHostService service, ILogger logger = null)
        {
            this.application = app ?? throw new ArgumentNullException(nameof(app));
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.logger = logger;
        }
        
        async public Task ExecuteAsync(CancellationToken token = default)
        {
            this.logger.LogDebug($"The subdirectory add as an app {this.application.Name}");

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
        }

        private void ConfigureApp(WebHostBuilderContext context, IApplicationBuilder app)
        {
            app.UseRouting()
               .UseEndpoints(endp =>
            {
                endp.MapControllers();
            });
        }

        private IConfiguration ConfigureAppConfiguration(IWebHostBuilder builder)
        { 
            var conf = new ConfigurationBuilder()
                    .SetBasePath(this.application.ApplicationPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
            builder.UseConfiguration(conf);
            return conf;
        }

        private void ConfigureServices(WebHostBuilderContext context, IServiceCollection svc)
        {
            var libraryAssembly = Assembly.LoadFile(this.LibraryFilename);

            svc.AddControllers();
            svc.AddMvc()
               .AddApplicationPart(libraryAssembly);
        }
    }
}