namespace Akinox.AspNetCore.SignalR.Msmq.Internal
{
    public readonly struct MsmqGroupCommand
    {
        /// <summary>
        /// Gets the ID of the group command.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the name of the server that sent the command.
        /// </summary>
        public string ServerName { get; }

        /// <summary>
        /// Gets the action to be performed on the group.
        /// </summary>
        public GroupAction Action { get; }

        /// <summary>
        /// Gets the group on which the action is performed.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Gets the ID of the connection to be added or removed from the group.
        /// </summary>
        public string ConnectionId { get; }

        public MsmqGroupCommand(int id, string serverName, GroupAction action, string groupName, string connectionId)
        {
            this.Id = id;
            this.ServerName = serverName;
            this.Action = action;
            this.GroupName = groupName;
            this.ConnectionId = connectionId;
        }
    }
}
