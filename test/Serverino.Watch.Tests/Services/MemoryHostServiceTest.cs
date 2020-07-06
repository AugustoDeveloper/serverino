using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Moq;
using Serverino.Watch.Models;
using Serverino.Watch.Services;
using Xunit;

namespace Serverino.Watch.Tests.Services
{
    public class MemoryHostServiceTest
    {
        [Fact]
        public void When_Call_Constructor_With_Null_Argument_Should_Returns_ArgumentNullException()
        {
            Func<IHostService> constructorWithNullArgument = () => new MemoryHostService(null);
            constructorWithNullArgument.Should().ThrowExactly<ArgumentNullException>();
        }

        [Fact]
        public void When_Call_AddNewHost_Pass_Null_Argument_Should_Throws_ArgumentNullException()
        {
            Action addNewHostWithNullApplication = () => new MemoryHostService().AddNewHost(null, null);
            Action addNewHostWithNullHost= () => new MemoryHostService().AddNewHost(new Application("app", AppContext.BaseDirectory, DateTime.Now), null);

            addNewHostWithNullApplication.Should().ThrowExactly<ArgumentNullException>();
            addNewHostWithNullHost.Should().ThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void When_Call_AddNewHost_Pass_Valid_Argument_Should_Add_On_Dictionary()
        {
            var hosts = new Dictionary<Guid, IHost>();
            var app = new Application("app", AppContext.BaseDirectory, DateTime.Now);
            new MemoryHostService(hosts).AddNewHost(app, new Mock<IHost>().Object);

            app.IsHosted.Should().BeTrue();
            hosts.ContainsKey(app.HostedKey).Should().BeTrue();
            hosts[app.HostedKey].Should().NotBeNull();
        }

        [Fact]
        public void When_Call_AddNewHost_Pass_Valid_Argument_And_Not_Exist_On_Hosts_Should_Add_On_Dictionary()
        {
            var hosts = new Dictionary<Guid, IHost>();
            var app = new Application("app", AppContext.BaseDirectory, DateTime.Now);
            new MemoryHostService(hosts).AddNewHost(app, new Mock<IHost>().Object);

            app.IsHosted.Should().BeTrue();
            hosts.ContainsKey(app.HostedKey).Should().BeTrue();
            hosts[app.HostedKey].Should().NotBeNull();
        }
        
        [Fact]
        public void When_Call_RemoveHost_Pass_Invalid_Argument_Should_Throws_ArgumentNullException()
        {
            var hosts = new Dictionary<Guid, IHost>();
            var app = new Application("app", AppContext.BaseDirectory, DateTime.Now);
            Action removeHostAction = () => new MemoryHostService(hosts).RemoveHost(null);
            removeHostAction.Should().ThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void When_Call_RemoveHost_Pass_Valid_Argument_But_Not_Exist_Should_Do_Nothing()
        {
            var hosts = new Dictionary<Guid, IHost>();
            var app = new Application("app", AppContext.BaseDirectory, DateTime.Now);
            new MemoryHostService(hosts).RemoveHost(app);
        }
        
        [Fact]
        public void When_Call_RemoveHost_Pass_Valid_Argument_But_Exist_Should_Remove_Host_From_Dictionary()
        {
            var key = Guid.NewGuid();
            var hosts = new Dictionary<Guid, IHost>
            {
                { key, new Mock<IHost>().Object }
            };
            var app = new Application("app", AppContext.BaseDirectory, DateTime.Now);
            app.MarkHosted(key);
            new MemoryHostService(hosts).RemoveHost(app);

            hosts.Count.Should().Be(0);
        }

        [Fact]
        public void When_Call_GetByApp_Pass_Invalid_Argument_Should_Throws_ArgumentNullException()
        {
            var hosts = new Dictionary<Guid, IHost>();
            Action getByAppAction = () => new MemoryHostService(hosts).RemoveHost(null);
            getByAppAction.Should().ThrowExactly<ArgumentNullException>();
        }
        
        [Fact]
        public void When_Call_GetByApp_Pass_Valid_Argument_But_Not_Register_On_Dictionary_Should_Returns_Null()
        {
            var hosts = new Dictionary<Guid, IHost>();
            var app = new Application("app", AppContext.BaseDirectory, DateTime.Now);
            app.MarkHosted(Guid.NewGuid());

            var host = new MemoryHostService(hosts).GetByApp(app);
            host.Should().BeNull();
        }
        
        [Fact]
        public void When_Call_GetByApp_Pass_Valid_Argument_And_Register_On_Dictionary_Should_Returns_Null()
        {
            var key = Guid.NewGuid();
            var hosts = new Dictionary<Guid, IHost>
            {
                { key, new Mock<IHost>().Object }
            };
            var app = new Application("app", AppContext.BaseDirectory, DateTime.Now);
            app.MarkHosted(key);

            var host = new MemoryHostService(hosts).GetByApp(app);
            host.Should().NotBeNull();
        }
        
        [Fact]
        public void When_Call_GetAll_With_Empty_Dictionary_Should_Returns_Empty_Array()
        {
            var hosts = new Dictionary<Guid, IHost>
            {
            };
            var hostApps = new MemoryHostService(hosts).GetAll();
            hostApps.Should().BeEmpty();
        }
        
        [Fact]
        public void When_Call_GetAll_With_One_Item_Dictionary_Should_Returns_One_Item_Array()
        {
            var key = Guid.NewGuid();
            var hosts = new Dictionary<Guid, IHost>
            {
                { key, new Mock<IHost>().Object }
            };
            var hostApps = new MemoryHostService(hosts).GetAll();
            hostApps.Should().NotBeEmpty();
            hostApps.Should().HaveCount(1);
            hosts[key].Should().NotBeNull();
        }
        
        [Fact]
        public void When_Call_Dispose_Should_Clean_Dictionary()
        {
            var key = Guid.NewGuid();
            var hosts = new Dictionary<Guid, IHost>
            {
                { key, new Mock<IHost>().Object }
            };
            new MemoryHostService(hosts).Dispose();

            hosts.Should().BeEmpty();
        }
    }
}