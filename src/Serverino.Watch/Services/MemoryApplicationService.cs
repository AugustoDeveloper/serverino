using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serverino.Watch.Models;

namespace Serverino.Watch.Services
{
    public class MemoryApplicationService : IApplicationService
    {
        private readonly string appsFolder;
        private readonly Dictionary<string, Application> hostedApplications;
        private bool disposed;

        public MemoryApplicationService(string appsFolder) : this(appsFolder, new Dictionary<string, Application>()) { }

        public MemoryApplicationService(string appsFolder, Dictionary<string, Application> hostedApplications)
        {
            appsFolder = !string.IsNullOrWhiteSpace(appsFolder) ? appsFolder : throw new ArgumentNullException(nameof(appsFolder));
            this.appsFolder = Directory.Exists(appsFolder) ? appsFolder : throw new DirectoryNotFoundException($"The apps directory not exits ");
            this.hostedApplications = hostedApplications ?? throw new ArgumentNullException(nameof(hostedApplications));
        }
        
        public Application[] GetNotHostedApplications()
        {
            var subdirectories = Directory.GetDirectories(this.appsFolder);
            if (subdirectories.Length == 0)
            {
                return new Application[]{ };
            }

            var directories = subdirectories
                .Where(di => !di.Equals(".ignore", StringComparison.InvariantCultureIgnoreCase))
                .Where(di => Directory.GetFiles(di).Length > 0)
                .Select(s => new DirectoryInfo(s));

            return directories
                .Where(di => !this.hostedApplications.ContainsKey(di.Name))
                .Where(di => Directory.GetFiles(di.FullName).Any(f =>
                    f.EndsWith($"{di.Name}.dll", StringComparison.InvariantCultureIgnoreCase)))
                .Where(di => Directory.GetFiles(di.FullName).Any(f =>
                    f.EndsWith($"AppSettings.json", StringComparison.InvariantCultureIgnoreCase)))
                .Select(di => new Application(di.Name, di.FullName, di.LastWriteTimeUtc))
                .ToArray();
        }

        public Application[] GetRemovedApplications()
        {
            var subdirectories = Directory.GetDirectories(this.appsFolder)
                .Where(di => !di.Equals(".ignore", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
            if (subdirectories.Length == 0)
            {
                return this.hostedApplications.Values.ToArray();
            }

            var directories = subdirectories
            .Select(s => new DirectoryInfo(s));

            return this.hostedApplications.Values
            .Where(app => directories.All(di => di.Name != app.Name))
            .ToArray();
        }

        public Application[] GetUpdatedApplications()
        {
            var subdirectories = Directory.GetDirectories(this.appsFolder)
                .Where(di => !di.Equals(".ignore", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
            if (subdirectories.Length == 0)
            {
                return new Application[]{ };
            }

            var directories = subdirectories
                .Where(di => Directory.GetFiles(di).Length > 0)
                .Select(s => new DirectoryInfo(s))
                .ToArray();
            

            return directories
            .Where(di => this.hostedApplications.ContainsKey(di.Name) && 
                        di.LastWriteTimeUtc > this.hostedApplications[di.Name].ModifiedAt)
            .Where(di => Directory.GetFiles(di.FullName).Any(f =>
                f.EndsWith($"{di.Name}.dll", StringComparison.InvariantCultureIgnoreCase)))
            .Where(di => Directory.GetFiles(di.FullName).Any(f =>
                f.EndsWith($"AppSettings.json", StringComparison.InvariantCultureIgnoreCase)))
            .Select(di => this.hostedApplications[di.Name])
            .ToArray();
        }

        public void PersistHostedApplications(params Application[] applications)
        {
            applications = applications ?? throw new ArgumentNullException(nameof(applications));
            foreach(var app in applications)
            {
                this.hostedApplications.TryAdd(app.Name, app);
            }
        }

        public void RemoveHostedApplication(string appName)
        {
            appName = appName ?? throw new ArgumentNullException(nameof(appName));
            if(this.hostedApplications.ContainsKey(appName))
            {
                this.hostedApplications.Remove(appName);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || this.disposed)
            {
                return;
            }
            
            this.hostedApplications.Clear();
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}