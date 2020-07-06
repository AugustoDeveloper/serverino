using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Serverino.Watch.Models;

namespace Serverino.Watch.Services
{
    public class MemoryHostService : IHostService
    {
        private readonly Dictionary<Guid, IHost> hosts;
        private bool disposed;

        public MemoryHostService() : this(new Dictionary<Guid, IHost>()) {}
        public MemoryHostService(Dictionary<Guid, IHost> hosts)
        {
            this.hosts = hosts ?? throw new ArgumentNullException(nameof(hosts));
        }

        public void AddNewHost(Application app, IHost host)
        {
            app = app ?? throw new ArgumentNullException(nameof(app));
            host = host ?? throw new ArgumentNullException(nameof(host));
            var key = Guid.NewGuid();
            
            if (!this.hosts.TryAdd(key, host))
            {
                throw new ArgumentException("The key is already register");
            }
            
            app.MarkHosted(key);
        }

        public void RemoveHost(Application app)
        {
            app = app ?? throw new ArgumentNullException(nameof(app));
            this.hosts.Remove(app.HostedKey);
        }

        public IHost GetByApp(Application application)
        {
            application = application ?? throw new ArgumentNullException(nameof(application));
            
            return this.hosts.ContainsKey(application.HostedKey) ? 
                this.hosts[application.HostedKey] :
                null;
        }

        public IHost[] GetAll()
            => this.hosts.Values.ToArray();

        private void Dispose(bool disposing)
        {
            if (disposing && !this.disposed)
            {
                this.hosts.Clear();
                this.disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}