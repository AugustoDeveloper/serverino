using System;
using System.IO;

namespace Serverino.Watch.Models
{
    public class Application
    {
        public Guid HostedKey { get; private set; }
        public DateTime HostedAt { get; private set; }
        public TimeSpan HostedTime => (DateTime.UtcNow - HostedAt);
        public bool IsHosted { get; private set; }
        public string Name { get; }
        public string ApplicationPath { get; }
        public string ConfigurationFilename => Path.Combine(this.ApplicationPath, "AppSettings.json");
        public string MainLibraryFilename => Path.Combine(this.ApplicationPath, $"{this.Name}.dll");
        public DateTime ModifiedAt { get; private set; }
        public int Port { get; set; }

        public Application(string name, string path, DateTime lastModification)
        {
            this.Name = name;
            this.ApplicationPath = path;
            this.ModifiedAt = lastModification;
        }

        public void MarkHosted(Guid hostedkey)
        {
            this.ModifiedAt = DateTime.UtcNow;
            this.HostedKey = hostedkey;
            this.HostedAt = DateTime.UtcNow;
            this.IsHosted = true;
        }
    }
}