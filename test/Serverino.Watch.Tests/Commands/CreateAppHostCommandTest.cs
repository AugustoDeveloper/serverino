using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Serverino.Watch.Commands;
using Serverino.Watch.Commands.Exceptions;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using Serverino.Watch.Tests.Services;
using Xunit;

namespace Serverino.Watch.Tests.Commands
{
    public class CreateAppHostCommandTest : IClassFixture<AppsFolderFixture>
    {
        private AppsFolderFixture folderFixture;

        public CreateAppHostCommandTest(AppsFolderFixture folderFixture)
        {
            this.folderFixture = folderFixture;
            folderFixture.AppsFolder = "AsyncApps";
        }
        
        [Fact]
        public void When_Call_Constructor_With_Invalid_Argument_Should_Throws_ArgumentNullException()
        {
            Func<IAsyncCommand> constructorWithAllNullArgumentAction = () => new CreateAppHostCommand(null, null, null);
            Func<IAsyncCommand> constructorWithNullApp = () =>
                new CreateAppHostCommand(null, new Mock<IApplicationService>().Object, new Mock<IHostService>().Object);
            Func<IAsyncCommand> constructorWithNullAppService = () =>
                new CreateAppHostCommand(new Application("app", AppContext.BaseDirectory, DateTime.Now), null,
                    new Mock<IHostService>().Object);
            Func<IAsyncCommand> constructorWithNullHostService = () =>
                new CreateAppHostCommand(new Application("app", AppContext.BaseDirectory, DateTime.Now),
                    new Mock<IApplicationService>().Object, null);

            constructorWithAllNullArgumentAction.Should().ThrowExactly<ArgumentNullException>();
            constructorWithNullApp.Should().ThrowExactly<ArgumentNullException>();
            constructorWithNullAppService.Should().ThrowExactly<ArgumentNullException>();
            constructorWithNullHostService.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public async Task When_Call_ExecuteAsync_With_Invalid_Path_Files_On_Application_Should_Returns_Exception()
        {
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder1 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp1");
            var sampleAppFolder2 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp2");
            using var jsonFile = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "AppSettings.json");
            using var libFile = this.folderFixture.CreateFileOnDirectory(sampleAppFolder2, $"{sampleAppFolder2.Name}.dll");

            Func<Task> constructorWithAppWithInvalidPath = () =>
                new CreateAppHostCommand(new Application("app", Path.Combine(this.folderFixture.AppsFolder, "NotExistApp"), DateTime.Now),
                    new Mock<IApplicationService>().Object, new Mock<IHostService>().Object).ExecuteAsync();
            Func<Task> constructorWithAppWithNotExistLibFile = () =>
                new CreateAppHostCommand(new Application("SampleApp1", sampleAppFolder1.FullName, DateTime.Now),
                    new Mock<IApplicationService>().Object, new Mock<IHostService>().Object).ExecuteAsync();
            Func<Task> constructorWithAppWithNotExistConfFile = () =>
                new CreateAppHostCommand(new Application("SampleApp2", sampleAppFolder2.FullName, DateTime.Now),
                    new Mock<IApplicationService>().Object, new Mock<IHostService>().Object).ExecuteAsync();

            var And = await constructorWithAppWithInvalidPath.Should()
                .ThrowExactlyAsync<InvalidCommandExectutionException>();
            And.WithInnerExceptionExactly<DirectoryNotFoundException>();
            
            And = await constructorWithAppWithNotExistLibFile.Should()
                .ThrowExactlyAsync<InvalidCommandExectutionException>();
            And.WithInnerExceptionExactly<FileNotFoundException>();

            And = await constructorWithAppWithNotExistConfFile.Should()
                .ThrowExactlyAsync<InvalidCommandExectutionException>();
            And.WithInnerExceptionExactly<FileNotFoundException>();
        }

        [Fact]
        public async Task When_Port_Is_Using_Should_Return_Throws_InvalidOperationException()
        {
            int port = 5892;
            var listener = new TcpListener(IPAddress.Any, 5892);
            this.folderFixture.RecreateAppsFolder();
            var sampleAppFolder1 = this.folderFixture.CreateSubDirectoryOnApps("SampleApp1");
            
            this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, "AppSettings.json", $"{{\"port\": {port}}}");
            await using var libFile = this.folderFixture.CreateFileOnDirectory(sampleAppFolder1, $"{sampleAppFolder1.Name}.dll");
            
            try
            {
                listener.Start();
                var mockAppService = new Mock<IApplicationService>(MockBehavior.Loose);
                var mockService = new Mock<IHostService>(MockBehavior.Loose);
            
                var command = new CreateAppHostCommand(
                    new Application(sampleAppFolder1.Name, sampleAppFolder1.FullName, sampleAppFolder1.LastWriteTimeUtc),
                    mockAppService.Object,
                    mockService.Object);

                Func<Task> executeAsyncFunc = () => command.ExecuteAsync();
                await executeAsyncFunc.Should().ThrowExactlyAsync<InvalidCommandExectutionException>();
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}