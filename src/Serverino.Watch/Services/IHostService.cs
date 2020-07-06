using System;
using Microsoft.Extensions.Hosting;
using Serverino.Watch.Models;

namespace Serverino.Watch.Services
{
    public interface IHostService : IDisposable
    {
        void AddNewHost(Application app, IHost host);
        void RemoveHost(Application app);
        IHost GetByApp(Application application);
        IHost[] GetAll();
    }
}