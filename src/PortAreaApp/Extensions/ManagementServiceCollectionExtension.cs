using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortAreaApp.HostedServices;

namespace PortAreaApp.Extensions
{
    static public class ManagementServiceCollectionExtension
    {
        static public IServiceCollection AddManagementUI(this IServiceCollection service, IConfiguration configuration)
            => configuration.GetValue("UseManagement", false)
                ? service.AddHostedService<ManagementHostingWorker>()
                : service;
    }
}