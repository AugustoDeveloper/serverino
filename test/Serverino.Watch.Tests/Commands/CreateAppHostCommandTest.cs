using System;
using FluentAssertions;
using Moq;
using Serverino.Watch.Commands;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using Xunit;

namespace Serverino.Watch.Tests.Commands
{
    public class CreateAppHostCommandTest
    {
        [Fact]
        public void When_Call_Constructor_With_Invalid_Argument_Should_Throws_ArgumentNullException()
        {
            Func<IAsyncCommand> constructorWithAllNullArgumentAction = () => new CreateAppHostCommand(null, null);
            Func<IAsyncCommand> constructorWithNullApp = () => new CreateAppHostCommand(null, new Mock<IHostService>().Object);
            Func<IAsyncCommand> constructorWithNullService = () =>
                new CreateAppHostCommand(new Application("app", AppContext.BaseDirectory, DateTime.Now), null);

            constructorWithAllNullArgumentAction.Should().ThrowExactly<ArgumentNullException>();
            constructorWithNullApp.Should().ThrowExactly<ArgumentNullException>();
            constructorWithNullService.Should().ThrowExactly<ArgumentNullException>();
        }
        
        //TODO: Create all unit tests
    }
}