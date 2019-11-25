using System;
using System.Linq;
using System.Threading.Tasks;
using Akinox.AspNetCore.SignalR.Msmq.Implementations;
using Experimental.System.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace Akinox.AspNetCore.SignalR.Msmq.Test.Unit.Implementations
{
    public class GivenBasicMsmqBus : BaseTest
    {
        private readonly BasicMsmqBus bus;

        public GivenBasicMsmqBus()
        {
            var options = new MsmqOptions()
            {
                ApplicationName = "Akinox.AspNetCore.SignalR.Msmq.Test",
                ConnectionString = this.Configuration.GetConnectionString("Msmq")
            };

            this.bus = new BasicMsmqBus(Options.Create(options));
        }

        [Fact]
        public async Task When_getting_all_queues_Then_returns_some_values()
        {
            var queues = await this.bus.GetAllQueueNamesAsync();

            queues.Should().NotBeEmpty();
        }

        [Fact]
        public async Task When_publishing_Then_serializes_and_sends()
        {
            var guid = Guid.NewGuid().ToByteArray();
            await this.bus.PublishAsync("ohboy", guid);

            var allMessages = this.bus.GetMessages("ohboy");
            var message = allMessages.Last();

            message.Formatter = new BinaryMessageFormatter();
            var bytes = (byte[])message.Body;
            bytes.Should().BeEquivalentTo(guid);
        }
    }
}
