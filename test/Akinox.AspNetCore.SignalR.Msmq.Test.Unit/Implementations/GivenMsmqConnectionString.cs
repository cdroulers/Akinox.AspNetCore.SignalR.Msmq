using Akinox.AspNetCore.SignalR.Msmq.Implementations;
using FluentAssertions;
using Xunit;

namespace Akinox.AspNetCore.SignalR.Msmq.Test.Unit.Implementations
{
    public class GivenMsmqConnectionString
    {
        [Theory]
        [InlineData("FormatName:DIRECT=TCP:127.0.0.1\\private$", "TCP", "127.0.0.1", "private$", null)]
        [InlineData("FormatName:DIRECT=OS:servername\\private$\\test1", "OS", "servername", "private$", "test1")]
        [InlineData(".\\test1", null, ".", null, "test1")]
        public void When_parsing_Then_gets_parts(string connectionString, string directType, string host, string modifier, string queueName)
        {
            var parsed = MsmqConnectionString.Parse(connectionString);

            parsed.DirectType.Should().Be(directType);
            parsed.Host.Should().Be(host);
            parsed.Modifier.Should().Be(modifier);
            parsed.QueueName.Should().Be(queueName);
        }
    }
}
