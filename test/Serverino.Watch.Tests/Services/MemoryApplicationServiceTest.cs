using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using Xunit;

namespace Serverino.Watch.Tests.Services
{
    public class MemoryApplicationServiceTest
    {
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
            var emptyFolder = Path.Combine(AppContext.BaseDirectory, "EmptyFolder");
            Directory.CreateDirectory(emptyFolder);
            try
            {
                var service = new MemoryApplicationService(emptyFolder);
                var apps = service.GetNotHostedApplications();
                apps.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(emptyFolder);
            }
        }
        
        [Fact]
        public void When_Call_GetNotHostedApplications_On_Apps_Folder_With_One_SubFolder_SampleApp_Should_Returns_One_Item_On_Array()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            var sampleAppFolder = Path.Combine(appsFolder, "SampleApp");
            Directory.CreateDirectory(appsFolder);
            Directory.CreateDirectory(sampleAppFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder);
                var apps = service.GetNotHostedApplications();
                apps.Should().HaveCount(1);
                apps[0].Name.Should().Be("SampleApp");
                apps[0].ApplicationPath.Should().Be(sampleAppFolder);
            }
            finally
            {
                Directory.Delete(sampleAppFolder);
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_GetNotHostedApplications_On_Apps_Folder_With_One_SubFolder_SampleApp_And_This_App_Is_Already_Hosted_Should_Returns_Empty_Array()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            var sampleAppFolder = Path.Combine(appsFolder, "SampleApp");
            Directory.CreateDirectory(appsFolder);
            Directory.CreateDirectory(sampleAppFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder,
                    new Dictionary<string, Application>
                        {{"SampleApp", new Application("SampleApp", sampleAppFolder, DateTime.Now)}});

                var apps = service.GetNotHostedApplications();
                apps.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(sampleAppFolder);
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_GetRemovedApplications_On_Apps_Folder_With_Empty_SubFolder_Should_Returns_All_Items()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            var sampleAppFolder = Path.Combine(appsFolder, "SampleApp");
            
            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder, DateTime.Now)},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder, DateTime.Now)},
                {"SampleApp3", new Application("SampleApp3", sampleAppFolder, DateTime.Now)}
            };
            
            Directory.CreateDirectory(appsFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder, hostedApps);

                var apps = service.GetRemovedApplications();
                apps.Should().HaveCount(3);
            }
            finally
            {
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_GetRemovedApplications_On_Apps_Folder_With_Two_Folders_Apps_And_Three_Apps_Hosted_Should_Returns_One_Element()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            var sampleAppFolder1 = Path.Combine(appsFolder, "SampleApp1");
            var sampleAppFolder2 = Path.Combine(appsFolder, "SampleApp2");
            var sampleAppFolder3 = Path.Combine(appsFolder, "SampleApp3");
            
            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder1, DateTime.Now)},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder2, DateTime.Now)},
                {"SampleApp3", new Application("SampleApp3", sampleAppFolder3, DateTime.Now)}
            };
            
            Directory.CreateDirectory(appsFolder);
            Directory.CreateDirectory(sampleAppFolder1);
            Directory.CreateDirectory(sampleAppFolder2);
            try
            {
                var service = new MemoryApplicationService(appsFolder, hostedApps);

                var apps = service.GetRemovedApplications();
                apps.Should().HaveCount(1);
                apps[0].Name.Should().Be("SampleApp3");
                apps[0].ApplicationPath.Should().Be(sampleAppFolder3);
            }
            finally
            {
                Directory.Delete(sampleAppFolder2);
                Directory.Delete(sampleAppFolder1);
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_GetUpdatedApplications_On_Apps_Folder_And_Its_Empty_Should_Returns_Empty_Array()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");

            Directory.CreateDirectory(appsFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder, new Dictionary<string, Application>());

                var apps = service.GetUpdatedApplications();
                apps.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_GetUpdatedApplications_On_Apps_Folder_And_Has_Three_Apps_Not_Hosted_Should_Returns_Empty_Array()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            
            var sampleAppFolder1 = Path.Combine(appsFolder, "SampleApp1");
            var sampleAppFolder2 = Path.Combine(appsFolder, "SampleApp2");
            var sampleAppFolder3 = Path.Combine(appsFolder, "SampleApp3");

            
            Directory.CreateDirectory(appsFolder);
            var sampleApp1 = Directory.CreateDirectory(sampleAppFolder1);
            var sampleApp2 = Directory.CreateDirectory(sampleAppFolder2);
            var sampleApp3 = Directory.CreateDirectory(sampleAppFolder3);
            
            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder1, sampleApp1.LastWriteTimeUtc)},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder2, sampleApp2.LastWriteTimeUtc)},
                {"SampleApp3", new Application("SampleApp3", sampleAppFolder3, sampleApp3.LastWriteTimeUtc)}
            };

            Directory.CreateDirectory(appsFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder, hostedApps);

                var apps = service.GetUpdatedApplications();
                apps.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(sampleAppFolder3);
                Directory.Delete(sampleAppFolder2);
                Directory.Delete(sampleAppFolder1);
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_GetUpdatedApplications_On_Apps_Folder_And_Has_Three_Apps_And_Two_Hosted_Should_Returns_Empty_Array()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            
            var sampleAppFolder1 = Path.Combine(appsFolder, "SampleApp1");
            var sampleAppFolder2 = Path.Combine(appsFolder, "SampleApp2");
            var sampleAppFolder3 = Path.Combine(appsFolder, "SampleApp3");

            
            Directory.CreateDirectory(appsFolder);
            var sampleApp1 = Directory.CreateDirectory(sampleAppFolder1);
            var sampleApp2 = Directory.CreateDirectory(sampleAppFolder2);
            var sampleApp3 = Directory.CreateDirectory(sampleAppFolder3);
            
            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder1, sampleApp1.LastWriteTimeUtc)},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder2, sampleApp2.LastWriteTimeUtc)}
            };

            Directory.CreateDirectory(appsFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder, hostedApps);

                var apps = service.GetUpdatedApplications();
                apps.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(sampleAppFolder3);
                Directory.Delete(sampleAppFolder2);
                Directory.Delete(sampleAppFolder1);
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_GetUpdatedApplications_On_Apps_Folder_And_Has_Three_Apps_And_Two_Hosted_Was_Updated_Should_Returns_Two_Items_On_Array()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            
            var sampleAppFolder1 = Path.Combine(appsFolder, "SampleApp1");
            var sampleAppFolder2 = Path.Combine(appsFolder, "SampleApp2");
            var sampleAppFolder3 = Path.Combine(appsFolder, "SampleApp3");

            
            Directory.CreateDirectory(appsFolder);
            var sampleApp1 = Directory.CreateDirectory(sampleAppFolder1);
            var sampleApp2 = Directory.CreateDirectory(sampleAppFolder2);
            var sampleApp3 = Directory.CreateDirectory(sampleAppFolder3);
            
            var hostedApps = new Dictionary<string, Application>
            {
                {"SampleApp1", new Application("SampleApp1", sampleAppFolder1, DateTime.UtcNow.AddDays(-1))},
                {"SampleApp2", new Application("SampleApp2", sampleAppFolder2, DateTime.UtcNow.AddDays(-1))},
                {"SampleApp3", new Application("SampleApp3", sampleAppFolder3, sampleApp3.LastWriteTimeUtc)}
            };

            Directory.CreateDirectory(appsFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder, hostedApps);

                var apps = service.GetUpdatedApplications();
                apps.Should().HaveCount(2);
                apps[0].Name.Should().Be("SampleApp1");
                apps[1].Name.Should().Be("SampleApp2");
            }
            finally
            {
                Directory.Delete(sampleAppFolder3);
                Directory.Delete(sampleAppFolder2);
                Directory.Delete(sampleAppFolder1);
                Directory.Delete(appsFolder);
            }
        }

        [Fact]
        public void When_Call_PersistHostedApplications_Pass_Null_Applications_Should_Returns_ArgumentNullException()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            Directory.CreateDirectory(appsFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder, new Dictionary<string, Application>());

                Action persistAction = () => service.PersistHostedApplications(null);
                persistAction.Should().ThrowExactly<ArgumentNullException>();
            }
            finally
            {
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_PersistHostedApplications_Pass_Empty_Applications_Should_Returns_Should_Return_Nothing()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            Directory.CreateDirectory(appsFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder, new Dictionary<string, Application>());

                service.PersistHostedApplications(new Application[]{});
            }
            finally
            {
                Directory.Delete(appsFolder);
            }
        }
        
        [Fact]
        public void When_Call_PersistHostedApplications_Pass_One_Item_Applications_Should_Persist_On_Dictionary()
        {
            var appsFolder = Path.Combine(AppContext.BaseDirectory, "Apps");
            var hosted = new Dictionary<string, Application>();
            var appName = Guid.NewGuid().ToString();
            Directory.CreateDirectory(appsFolder);
            try
            {
                var service = new MemoryApplicationService(appsFolder, hosted);

                service.PersistHostedApplications(new Application[]{new Application(appName, AppContext.BaseDirectory, DateTime.Now), });

                hosted.Values.Should().HaveCount(1);
                hosted.ContainsKey(appName).Should().BeTrue();
                hosted[appName].Name.Should().Be(appName);
                hosted[appName].ApplicationPath.Should().Be(AppContext.BaseDirectory);
            }
            finally
            {
                Directory.Delete(appsFolder);
            }
        }
    }
}