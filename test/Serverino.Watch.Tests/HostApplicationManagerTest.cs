using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Serverino.Watch.Commands;
using Serverino.Watch.Commands.Exceptions;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using Xunit;

namespace Serverino.Watch.Tests
{
    public class HostApplicationManagerTest
    {
        [Fact]
        public void When_Call_Constructor_With_Invalid_Arguments_Should_Throws_Exceptions()
        {
            var mockAppService = new Mock<IApplicationService>(MockBehavior.Loose);
            var mockHostService = new Mock<IHostService>(MockBehavior.Loose);
            var mockFactory = new Mock<IFactoryAsyncCommand>(MockBehavior.Loose);
            var mockLogger = new Mock<ILogger<HostApplicationManager>>(MockBehavior.Loose);
            Func<HostApplicationManager> constructorWithAllNullArguments = () => new HostApplicationManager(null, null, null, null);
            Func<HostApplicationManager> constructorWithAppServiceNullArgument = () =>
                new HostApplicationManager(null, mockHostService.Object, mockFactory.Object, mockLogger.Object);
            Func<HostApplicationManager> constructorWithHostServiceNullArgument = () =>
                new HostApplicationManager(mockAppService.Object, null, mockFactory.Object, mockLogger.Object);
            Func<HostApplicationManager> constructorWithFactoryNullArgument = () =>
                new HostApplicationManager(mockAppService.Object, mockHostService.Object, null, mockLogger.Object);

            constructorWithAllNullArguments.Should().ThrowExactly<ArgumentNullException>();
            constructorWithAppServiceNullArgument.Should().ThrowExactly<ArgumentNullException>();
            constructorWithHostServiceNullArgument.Should().ThrowExactly<ArgumentNullException>();
            constructorWithFactoryNullArgument.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void When_Call_AddHost_With_Null_Argument_Should_Throws_ArgumentNullException()
        {
            var mockAppService = new Mock<IApplicationService>(MockBehavior.Loose);
            var mockHostService = new Mock<IHostService>(MockBehavior.Loose);
            var mockFactory = new Mock<IFactoryAsyncCommand>(MockBehavior.Loose);
            var mockLogger = new Mock<ILogger<HostApplicationManager>>(MockBehavior.Loose);

            Func<IHostManager<Application>> funcAddHostWithNullModel = () =>
                new HostApplicationManager(mockAppService.Object, mockHostService.Object, mockFactory.Object,
                    mockLogger.Object).AddHost(null);
            funcAddHostWithNullModel.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public async Task
            When_Call_AddHost_Adding_A_Command_Will_Throw_Exception_On_PersistAsync_Should_Returns_A_InvalidCommandExecutionException()
        {
            var app = new Application("app", AppContext.BaseDirectory, DateTime.Now);
            
            var mockAppService = new Mock<IApplicationService>();
            var mockHostService = new Mock<IHostService>();
            
            var mockCreateAppCommand = new Mock<IAsyncCommand>();
            
            mockCreateAppCommand
                .Setup(x => x.ExecuteAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidCommandExectutionException(nameof(CreateAppHostCommand), app));
            
            var mockFactory = new Mock<IFactoryAsyncCommand>();
            mockFactory
                .Setup(x => x.Create<CreateAppHostCommand>())
                .Returns(mockCreateAppCommand.Object);
            mockFactory
                .Setup(x => x.With(It.IsAny<Application>()))
                .Returns(mockFactory.Object);
            mockFactory
                .Setup(x => x.With(It.IsAny<IApplicationService>()))
                .Returns(mockFactory.Object);
            mockFactory
                .Setup(x => x.With(It.IsAny<IHostService>()))
                .Returns(mockFactory.Object);
            mockFactory
                .Setup(x => x.With(It.IsAny<ILogger>()))
                .Returns(mockFactory.Object);
            
            var mockLogger = new Mock<ILogger<HostApplicationManager>>();

            Func<Task> funcAddHostWithNullModel = () =>
                new HostApplicationManager(mockAppService.Object, mockHostService.Object, mockFactory.Object,
                    mockLogger.Object).AddHost(app).PersistAsync();
            await funcAddHostWithNullModel.Should().ThrowExactlyAsync<InvalidCommandExectutionException>();
        }
    }
}