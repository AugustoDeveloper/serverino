using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serverino.Watch.Models;

namespace Serverino.Watch.Services
{
    public class MemoryHostService : IHostService
    {
        private readonly Dictionary<Guid, IHost> hosts;
        private readonly ChannelWriter<string> applicationChannel;
        private bool disposed;

        public MemoryHostService(ChannelWriter<string> applicationChannel = null) : this(new Dictionary<Guid, IHost>(), applicationChannel) {}
        public MemoryHostService(Dictionary<Guid, IHost> hosts, ChannelWriter<string> applicationChannel = null)
        {
            this.hosts = hosts ?? throw new ArgumentNullException(nameof(hosts));
            this.applicationChannel = applicationChannel;
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
            this.applicationChannel.TryWrite(JsonConvert.SerializeObject(app));
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