using System;
using Akinox.AspNetCore.SignalR.Msmq.Internal;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Xunit;

namespace Akinox.AspNetCore.SignalR.Msmq.Test.Unit.Internal
{
    public class GivenMsmqProtocol
    {
        private readonly MsmqProtocol protocol;

        private readonly DummyHubProtocol[] hubProtocols;

        public GivenMsmqProtocol()
        {
            this.hubProtocols = new[] { new DummyHubProtocol("p1"), new DummyHubProtocol("p2") };
            this.protocol = new MsmqProtocol(this.hubProtocols);
        }

        [Fact]
        public void When_serializing_and_deserializing_Then_works()
        {
            var bytes = this.protocol.WriteInvocation(
                "Hai",
                new object[] { 1, "test" },
                new[] { "connection" },
                new[] { "group1", "group2" },
                new[] { "user1", "user2" },
                new[] { "conn1", "conn2" });

            var invocation = this.protocol.ReadInvocation(bytes);
            this.AssertInvocationEqual(invocation, new MsmqInvocation(
                new[] { "connection" },
                new[] { "group1", "group2" },
                new[] { "user1", "user2" },
                new[] { "conn1", "conn2" },
                new SerializedHubMessage(new InvocationMessage("Hai", new object[] { 1, "test" }))));
        }

        private void AssertInvocationEqual(MsmqInvocation actual, MsmqInvocation expected)
        {
            actual.Connections.Should().BeEquivalentTo(expected.Connections);
            actual.ExcludedConnectionIds.Should().BeEquivalentTo(expected.ExcludedConnectionIds);
            actual.Groups.Should().BeEquivalentTo(expected.Groups);
            actual.Users.Should().BeEquivalentTo(expected.Users);
            actual.Message.Should().NotBeNull();
            var expectedInvocationMessage = expected.Message.Message as InvocationMessage;

            // Verify the deserialized object has the necessary serialized forms
            foreach (var hubProtocol in this.hubProtocols)
            {
                actual.Message.GetSerializedMessage(hubProtocol).ToArray()
                    .Should()
                    .BeEquivalentTo(expected.Message.GetSerializedMessage(hubProtocol).ToArray());

                var writtenMessages = hubProtocol.GetWrittenMessages();
                foreach (var actualMessage in writtenMessages)
                {
                    var invocation = Assert.IsType<InvocationMessage>(actualMessage);
                    invocation.Target.Should().Be(expectedInvocationMessage.Target);
                    invocation.Arguments.Should().BeEquivalentTo(expectedInvocationMessage.Arguments);
                }
            }
        }
    }
}
