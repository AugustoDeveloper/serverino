using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using Xunit;

namespace Serverino.Watch.Tests.Services
{
    public class MemoryApplicationServiceTest : IClassFixture<AppsFolderFixture>
    {
        private AppsFolderFixture folderFixture;

        public MemoryApplicationServiceTest(AppsFolderFixture folderFixture)
        {
            this.folderFixture = folderFixture;
        }
        [Fact]
        public void When_Pass_Argument_Is_Invalid_To_Constructor_Should_Throws_Exceptions()
        {
            Func<MemoryApplicationService> constructorWithNullArgument = () => new MemoryApplicationService(null);
            Func<MemoryApplicationService> constructorWithEmptyArgument = () => new MemoryApplicationService(string.Empty);
            Func<MemoryApplicationService> constructorWithBlankArgument = () => new MemoryApplicationService(" ");
            Func<MemoryApplicationService> constructorWithNotExistentDirectoryArgument = () => new MemoryApplicationService(Guid.NewGuid().ToString());
            Func<MemoryApplicationService> constructorWithNullAppsHostedArgument = () => new MemoryApplicationService(AppContext.BaseDirectory, null);

            constructorWithNullArgument.Should().ThrowExactly<ArgumentNullException>();
            constructorWithEmptyArgument.Should().ThrowExactly<ArgumentNullException>();
            constructorWithBlankArgument.Should().ThrowExactly<ArgumentNullException>();
            constructorWithNotExistentDirectoryArgument.Should().ThrowExactly<DirectoryNotFoundException>();
            constructorWithNullAppsHostedArgument.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void When_Call_GetNotHostedApplications_On_Empty_Folder_Should_Returns_Empty_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder);
            var apps = service.GetNotHostedApplications();
            apps.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Call_GetNotHostedApplications_On_Apps_Folder_With_One_Empty_SubFolder_SampleApp_Should_Returns_Empty_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder = this.folderFixture.CreateSubDirectoryOnApps("SampleApp");
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder);
            var apps = service.GetNotHostedApplications();
            apps.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Call_GetNotHostedApplications_On_Apps_Folder_With_One_SubFolder_SampleApp_With_A_TextFile_Without_Dll_Or_Config_File_Should_Returns_Empty_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder = this.folderFixture.CreateSubDirectoryOnApps("SampleApp");
            using var textFile = this.folderFixture.CreateFileOnDirectory(sampleAppFolder, "notepad.txt");
            
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder);
            var apps = service.GetNotHostedApplications();
            apps.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Call_GetNotHostedApplications_On_Apps_Folder_With_One_SubFolder_SampleApp_With_Dll_And_Config_File_Should_Returns_One_Item_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder = this.folderFixture.CreateSubDirectoryOnApps("SampleApp");
            using var libFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder, "SampleApp.dll");
            using var configFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder, "AppSettings.json");
            
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder);
            var apps = service.GetNotHostedApplications();
            apps.Should().NotBeEmpty();
            apps[0].Name.Should().Be("SampleApp");
        }
        
        [Fact]
        public void When_Call_GetNotHostedApplications_On_Apps_Folder_With_One_SubFolder_SampleApp_And_This_App_Is_Already_Hosted_Should_Returns_Empty_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder = this.folderFixture.CreateSubDirectoryOnApps("SampleApp");
            
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder,
                new Dictionary<string, Application>
                    {{"SampleApp", new Application("SampleApp", sampleAppFolder.FullName, DateTime.Now)}});

            var apps = service.GetNotHostedApplications();
            apps.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Call_GetRemovedApplications_On_Apps_Folder_With_Empty_SubFolder_Should_Returns_All_Items()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder = this.folderFixture.CreateSubDirectoryOnApps("SampleApp");
            
            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder.FullName, DateTime.Now)},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder.FullName, DateTime.Now)},
                {"SampleApp3", new Application("SampleApp3", sampleAppFolder.FullName, DateTime.Now)}
            };
            
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, hostedApps);

            var apps = service.GetRemovedApplications();
            apps.Should().HaveCount(3);
        }
        
        [Fact]
        public void When_Call_GetRemovedApplications_On_Apps_Folder_With_Two_Folders_Apps_And_Three_Apps_Hosted_Should_Returns_One_Element()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder1 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp1");
            var sampleAppFolder2 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp2");
            var sampleAppFolder3 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp3");
            
            using var sampleAppOneFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "SampleApp1.dll");
            using var sampleAppTwoFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, "SampleApp2.dll");

            using var appConfigOneFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "AppSettings.json");
            using var appConfigTwoFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, "AppSettings.json");

            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder1.FullName, DateTime.Now)},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder2.FullName, DateTime.Now)},
                {"SampleApp3", new Application("SampleApp3", sampleAppFolder3.FullName, DateTime.Now)}
            };
            
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, hostedApps);
            
            Directory.Delete(sampleAppFolder3.FullName);
            var apps = service.GetRemovedApplications();
            apps.Should().HaveCount(1);
            apps[0].Name.Should().Be("SampleApp3");
            apps[0].ApplicationPath.Should().Be(sampleAppFolder3.FullName);
        }
        
        [Fact]
        public void When_Call_GetUpdatedApplications_On_Apps_Folder_And_Its_Empty_Should_Returns_Empty_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, new Dictionary<string, Application>());

            var apps = service.GetUpdatedApplications();
            apps.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Call_GetUpdatedApplications_On_Apps_Folder_And_Has_Three_Apps_Not_Hosted_Should_Returns_Empty_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder1 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp1");
            var sampleAppFolder2 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp2");
            var sampleAppFolder3 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp3");
            
            using var sampleAppOneFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "SampleApp1.dll");
            using var sampleAppTwoFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, "SampleApp2.dll");
            using var sampleAppThreeFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder3, "SampleApp3.dll");
            
            using var appConfigOneFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "AppSettings.json");
            using var appConfigTwoFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, "AppSettings.json");
            using var appConfigThreeFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder3, "AppSettings.json");
            
            var hostedApps = new Dictionary<string, Application>
            {
            };
            
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, hostedApps);

            var apps = service.GetUpdatedApplications();
            apps.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Call_GetUpdatedApplications_On_Apps_Folder_And_Has_Three_Apps_And_Two_Hosted_Should_Returns_Empty_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder1 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp1");
            var sampleAppFolder2 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp2");
            var sampleAppFolder3 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp3");
            
            using var sampleAppOneFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "SampleApp1.dll");
            using var sampleAppTwoFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, "SampleApp2.dll");
            using var sampleAppThreeFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder3, "SampleApp3.dll");
            
            using var appConfigOneFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "AppSettings.json");
            using var appConfigTwoFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, "AppSettings.json");
            using var appConfigThreeFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder3, "AppSettings.json");

            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder1.FullName, sampleAppFolder1.LastWriteTimeUtc)},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder2.FullName, sampleAppFolder2.LastWriteTimeUtc)}
            };
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, hostedApps);

            var apps = service.GetUpdatedApplications();
            apps.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Call_GetUpdatedApplications_On_Apps_Folder_And_Has_Three_Apps_And_Two_Hosted_Was_Updated_Should_Returns_Two_Items_On_Array()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder1 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp1");
            var sampleAppFolder2 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp2");
            var sampleAppFolder3 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp3");
            
            using var sampleAppOneFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "SampleApp1.dll");
            using var sampleAppTwoFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, "SampleApp2.dll");
            using var sampleAppThreeFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder3, "SampleApp3.dll");
            
            using var appConfigOneFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "AppSettings.json");
            using var appConfigTwoFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, "AppSettings.json");
            using var appConfigThreeFileName = this.folderFixture.CreateFileOnDirectory(sampleAppFolder3, "AppSettings.json");
            
            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder1.FullName, DateTime.UtcNow.AddDays(-1))},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder2.FullName, DateTime.UtcNow.AddDays(-1))},
                {"SampleApp3", new Application("SampleApp3", sampleAppFolder3.FullName, sampleAppFolder3.LastWriteTimeUtc)}
            };

            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, hostedApps);

            var apps = service.GetUpdatedApplications();
            apps.Should().HaveCount(2);
            apps[0].Name.Should().Be("SampleApp1");
            apps[1].Name.Should().Be("SampleApp2");
        }

        [Fact]
        public void When_Call_PersistHostedApplications_Pass_Null_Applications_Should_Returns_ArgumentNullException()
        {
            this.folderFixture.RecreateAppsFolder();
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, new Dictionary<string, Application>());

            Action persistAction = () => service.PersistHostedApplications(null);
            persistAction.Should().ThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void When_Call_PersistHostedApplications_Pass_Empty_Applications_Should_Returns_Should_Do_Nothing()
        {
            this.folderFixture.RecreateAppsFolder();
            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, new Dictionary<string, Application>());

            service.PersistHostedApplications(new Application[]{});
        }
        
        [Fact]
        public void When_Call_PersistHostedApplications_Pass_One_Item_Applications_Should_Persist_On_Dictionary()
        {
            this.folderFixture.RecreateAppsFolder();
            var hosted = new Dictionary<string, Application>();
            var appName = Guid.NewGuid().ToString();

            var service = new MemoryApplicationService(this.folderFixture.AppsFolder, hosted);

            service.PersistHostedApplications(new Application[]{new Application(appName, AppContext.BaseDirectory, DateTime.Now), });

            hosted.Values.Should().HaveCount(1);
            hosted.ContainsKey(appName).Should().BeTrue();
            hosted[appName].Name.Should().Be(appName);
            hosted[appName].ApplicationPath.Should().Be(AppContext.BaseDirectory);
        }
    }
}