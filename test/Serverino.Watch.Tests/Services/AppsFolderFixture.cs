using System;
using System.IO;
using System.Text;

namespace Serverino.Watch.Tests.Services
{
    public class AppsFolderFixture : IDisposable
    {
        private string appsFolder;

        public string AppsFolder
        {
            get => Path.Combine(AppContext.BaseDirectory, appsFolder ?? "Apps");
            set => appsFolder = value;
        }

        public DirectoryInfo RecreateAppsFolder()
        {
            if (Directory.Exists(this.AppsFolder))
            {
                Directory.Delete(this.AppsFolder, true);
            }
            return Directory.CreateDirectory(this.AppsFolder);
        }

        public DirectoryInfo CreateSubDirectoryOnApps(string folderName)
            => Directory.CreateDirectory(Path.Combine(this.AppsFolder, folderName));

        public FileStream CreateFileOnDirectory(DirectoryInfo directory, string filename)
            => File.Create(Path.Combine(directory.FullName, filename));

        public void CreateFileOnDirectory(DirectoryInfo directory, string filename, string text)
            => File.AppendAllText(Path.Combine(directory.FullName, filename), text, Encoding.UTF8);
        
        public void Dispose()
        {
            if (Directory.Exists(this.AppsFolder))
            {
                Directory.Delete(this.AppsFolder, true);
            }
        }
    }
}