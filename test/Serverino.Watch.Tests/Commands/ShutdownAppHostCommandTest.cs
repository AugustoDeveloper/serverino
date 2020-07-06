using System;
using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Serverino.Watch.Commands;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using Xunit;

namespace Serverino.Watch.Tests.Commands
{
    public class ShutdownAppHostCommandTest
    {
        [Fact]
        public void When_Call_Constructor_With_Invalid_Argument_Should_Throws_ArgumentNullException()
        {
            Func<IAsyncCommand> constructorWithAllNullArgumentAction = () => new ShutdownAppHostCommand(null, null);
            Func<IAsyncCommand> constructorWithNullApp = () => new ShutdownAppHostCommand(null, new Mock<IHostService>().Object);
            Func<IAsyncCommand> constructorWithNullService = () =>
                new ShutdownAppHostCommand(new Application("app", AppContext.BaseDirectory, DateTime.Now), null);

            constructorWithAllNullArgumentAction.Should().ThrowExactly<ArgumentNullException>();
            constructorWithNullApp.Should().ThrowExactly<ArgumentNullException>();
            constructorWithNullService.Should().ThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void When_Call_ExecuteAsync_And_App_Not_Contains_Registered_Host_Should_Do_Nothing()
        {
            var mockService = new Mock<IHostService>();
            mockService
                .Setup(s => s.GetByApp(It.IsAny<Application>()))
                .Returns((IHost) null)
                .Verifiable();

            mockService
                .Setup(s => s.RemoveHost(It.IsAny<Application>()));

            var command = new ShutdownAppHostCommand(new Application("app", AppContext.BaseDirectory, DateTime.Now),
                mockService.Object);

            command.ExecuteAsync();

            mockService.Verify(s => s.GetByApp(It.IsAny<Application>()), Times.Once);
            mockService.Verify(s => s.RemoveHost(It.IsAny<Application>()), Times.Never);
        }
        
        [Fact]
        public void When_Call_ExecuteAsync_And_App_Registered_Host_Should_Do_Nothing()
        {
            var mockHost = new Mock<IHost>();
            mockHost
                .Setup(s => s.StopAsync(It.IsAny<CancellationToken>()))
                .Verifiable();
            
            var mockService = new Mock<IHostService>();
            mockService
                .Setup(s => s.GetByApp(It.IsAny<Application>()))
                .Returns(mockHost.Object)
                .Verifiable();

            mockService
                .Setup(s => s.RemoveHost(It.IsAny<Application>()));

            var command = new ShutdownAppHostCommand(new Application("app", AppContext.BaseDirectory, DateTime.Now),
                mockService.Object);

            command.ExecuteAsync();

            mockHost.Verify(s => s.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockService.Verify(s => s.GetByApp(It.IsAny<Application>()), Times.Once);
            mockService.Verify(s => s.RemoveHost(It.IsAny<Application>()), Times.Once);
        }
    }
}