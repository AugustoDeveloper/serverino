using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace PortAreaApp
{
    static public class ManagementBuilder
    {
        public static IHost CreateHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration(opt =>
                {
                    opt
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddEnvironmentVariables("SRVRN_");
                })
                .Build();
    }
}
