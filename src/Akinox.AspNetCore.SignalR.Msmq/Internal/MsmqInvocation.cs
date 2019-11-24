using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Akinox.AspNetCore.SignalR.Msmq.Internal
{
    public readonly struct MsmqInvocation
    {
        public IReadOnlyList<string> Connections { get; }

        public IReadOnlyList<string> Groups { get; }

        public IReadOnlyList<string> Users { get; }

        /// <summary>
        /// Gets a list of connections that should be excluded from this invocation.
        /// May be null to indicate that no connections are to be excluded.
        /// </summary>
        public IReadOnlyList<string> ExcludedConnectionIds { get; }

        /// <summary>
        /// Gets the message serialization cache containing serialized payloads for the message.
        /// </summary>
        public SerializedHubMessage Message { get; }

        public MsmqInvocation(
            IReadOnlyList<string> connections,
            IReadOnlyList<string> groups,
            IReadOnlyList<string> users,
            IReadOnlyList<string> excludedConnectionIds,
            SerializedHubMessage message)
        {
            this.Connections = connections;
            this.Groups = groups;
            this.Users = users;
            this.ExcludedConnectionIds = excludedConnectionIds;
            this.Message = message;
        }

        public static MsmqInvocation Create(
            string target,
            object[] arguments,
            IReadOnlyList<string> connections,
            IReadOnlyList<string> groups,
            IReadOnlyList<string> users,
            IReadOnlyList<string> excludedConnectionIds)
        {
            return new MsmqInvocation(
                connections,
                groups,
                users,
                excludedConnectionIds,
                new SerializedHubMessage(new InvocationMessage(target, null, arguments)));
        }
    }
}
