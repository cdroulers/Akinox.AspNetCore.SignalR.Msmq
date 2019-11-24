using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akinox.AspNetCore.SignalR.Msmq.Abstractions;
using Akinox.AspNetCore.SignalR.Msmq.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Akinox.AspNetCore.SignalR.Msmq
{
    public class MsmqHubLifetimeManager<THub> : HubLifetimeManager<THub>, IDisposable
        where THub : Hub
    {
        private readonly HubConnectionStore connections = new HubConnectionStore();
        private readonly ILogger logger;
        private readonly MsmqOptions options;
        private readonly MsmqSubscriptionManager groups = new MsmqSubscriptionManager();
        private readonly MsmqSubscriptionManager users = new MsmqSubscriptionManager();
        private readonly MsmqProtocol protocol;
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1);
        private readonly IMsmqBus msmqBus;
        private IMsmqChannel msmqChannel;
        private readonly MsmqQueues queues;
        private readonly AckHandler ackHandler;
        private int internalId;

        public MsmqHubLifetimeManager(
            ILogger<MsmqHubLifetimeManager<THub>> logger,
            IOptions<MsmqOptions> options,
            IHubProtocolResolver hubProtocolResolver,
            IMsmqBus msmqBus)
        {
            this.logger = logger;
            this.options = options.Value;
            this.protocol = new MsmqProtocol(hubProtocolResolver.AllProtocols);
            this.msmqBus = msmqBus;
            this.queues = new MsmqQueues(this.options.ApplicationName);

            MsmqLog.ConnectingToEndpoints(this.logger, this.options.ConnectionString, this.options.ApplicationName);
            _ = this.EnsureMsmqServerConnection();
        }

        public override async Task OnConnectedAsync(HubConnectionContext connection)
        {
            await this.EnsureMsmqServerConnection();

            this.connections.Add(connection);

            if (!string.IsNullOrWhiteSpace(connection.UserIdentifier))
            {
                await this.users.AddSubscriptionAsync(connection.UserIdentifier, connection, (channelName, subscriptions) => Task.CompletedTask);
            }
        }

        public override async Task OnDisconnectedAsync(HubConnectionContext connection)
        {
            this.connections.Remove(connection);

            if (!string.IsNullOrEmpty(connection.UserIdentifier))
            {
                await this.RemoveUserAsync(connection);
            }
        }

        public override Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            var message = this.protocol.WriteInvocation(methodName, args, null, null, null, null);
            return this.PublishAsync(message);
        }

        public override Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            var message = this.protocol.WriteInvocation(methodName, args, null, null, null, excludedConnectionIds);
            return this.PublishAsync(message);
        }

        public override Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException(nameof(connectionId));
            }

            // If the connection is local we can skip sending the message through the bus since we require sticky connections.
            // This also saves serializing and deserializing the message!
            var connection = this.connections[connectionId];
            if (connection != null)
            {
                return connection.WriteAsync(new InvocationMessage(methodName, args)).AsTask();
            }

            var message = this.protocol.WriteInvocation(methodName, args, new[] { connectionId }, null, null, null);
            return this.PublishAsync(message);
        }

        public override Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var message = this.protocol.WriteInvocation(methodName, args, null, new[] { groupName }, null, null);
            return this.PublishAsync(message);
        }

        public override Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var message = this.protocol.WriteInvocation(methodName, args, null, new[] { groupName }, null, excludedConnectionIds);
            return this.PublishAsync(message);
        }

        public override Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            var message = this.protocol.WriteInvocation(methodName, args, null, null, new[] { userId }, null);
            return this.PublishAsync(message);
        }

        public override Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException(nameof(connectionId));
            }

            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var connection = this.connections[connectionId];
            if (connection != null)
            {
                // short circuit if connection is on this server
                return this.AddGroupAsyncCore(connection, groupName);
            }

            return this.SendGroupActionAndWaitForAck(connectionId, groupName, GroupAction.Add);
        }

        public override Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            if (connectionId == null)
            {
                throw new ArgumentNullException(nameof(connectionId));
            }

            if (groupName == null)
            {
                throw new ArgumentNullException(nameof(groupName));
            }

            var connection = this.connections[connectionId];
            if (connection != null)
            {
                // short circuit if connection is on this server
                return this.RemoveGroupAsyncCore(connection, groupName);
            }

            return this.SendGroupActionAndWaitForAck(connectionId, groupName, GroupAction.Remove);
        }

        public override async Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            if (connectionIds == null)
            {
                throw new ArgumentNullException(nameof(connectionIds));
            }

            var publishTasks = new List<Task>(connectionIds.Count);
            var payload = this.protocol.WriteInvocation(methodName, args, connectionIds, null, null, null);

            await this.PublishAsync(payload);
        }

        public override async Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            if (groupNames == null)
            {
                throw new ArgumentNullException(nameof(groupNames));
            }

            var publishTasks = new List<Task>(groupNames.Count);
            var payload = this.protocol.WriteInvocation(methodName, args, null, groupNames, null, null);

            await this.PublishAsync(payload);
        }

        public override async Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            if (userIds.Count > 0)
            {
                var payload = this.protocol.WriteInvocation(methodName, args, null, null, userIds, null);
                await this.PublishAsync(payload);
            }
        }

        private async Task PublishAsync(byte[] payload)
        {
            await this.EnsureMsmqServerConnection();
            var channels = await this.msmqBus.GetAllQueueNames();
            foreach (var channel in channels.Where(x => x.StartsWith(this.queues.Invocations(string.Empty))))
            {
                MsmqLog.PublishToChannel(this.logger, channel);
                await this.msmqBus.PublishAsync(channel, payload);
            }
        }

        private async Task PublishGroupCommandAsync(byte[] payload)
        {
            var channels = await this.msmqBus.GetAllQueueNames();
            foreach (var channel in channels.Where(x => x.StartsWith(this.queues.GroupManagement(string.Empty))))
            {
                MsmqLog.PublishToChannel(this.logger, channel);
                await this.msmqBus.PublishAsync(channel, payload);
            }
        }

        private Task AddGroupAsyncCore(HubConnectionContext connection, string groupName)
        {
            return this.groups.AddSubscriptionAsync(groupName, connection, (n, s) => Task.CompletedTask);
        }

        /// <summary>
        /// This takes <see cref="HubConnectionContext"/> because we want to remove the connection from the
        /// _connections list in OnDisconnectedAsync and still be able to remove groups with this method.
        /// </summary>
        private async Task RemoveGroupAsyncCore(HubConnectionContext connection, string groupName)
        {
            await this.groups.RemoveSubscriptionAsync(groupName, connection, channelName =>
            {
                MsmqLog.Unsubscribe(this.logger, channelName);
                return Task.CompletedTask;
            });
        }

        private async Task SendGroupActionAndWaitForAck(string connectionId, string groupName, GroupAction action)
        {
            var id = Interlocked.Increment(ref this.internalId);

            var ack = this.ackHandler.CreateAck(id);

            // Send Add/Remove Group to other servers and wait for an ack or timeout
            var message = this.protocol.WriteGroupCommand(new MsmqGroupCommand(id, Environment.MachineName, action, groupName, connectionId));
            await this.PublishGroupCommandAsync(message);

            await ack;
        }

        private Task RemoveUserAsync(HubConnectionContext connection)
        {
            return this.users.RemoveSubscriptionAsync(connection.UserIdentifier, connection, channelName =>
            {
                MsmqLog.Unsubscribe(this.logger, "user:" + connection.UserIdentifier);
                return Task.CompletedTask;
            });
        }

        public void Dispose()
        {
        }

        private async Task EnsureMsmqServerConnection()
        {
            if (this.msmqChannel == null)
            {
                await this.connectionLock.WaitAsync();
                try
                {
                    if (this.msmqChannel == null)
                    {
                        this.msmqChannel = await this.msmqBus.SubscribeAsync(this.queues.Invocations(Environment.MachineName));
                        MsmqLog.Connected(this.logger);
                        this.msmqChannel.OnMessage(this.HandleRegularMessage);

                        var msmqGroupChannel = await this.msmqBus.SubscribeAsync(this.queues.GroupManagement(Environment.MachineName));
                        msmqGroupChannel.OnMessage(this.HandleGroupMessage);

                        var msmqAckChannel = await this.msmqBus.SubscribeAsync(this.queues.Ack(Environment.MachineName));
                        msmqAckChannel.OnMessage(this.HandleAckMessage);
                    }
                }
                catch (Exception e)
                {
                    MsmqLog.ConnectionFailed(this.logger, e);
                }
                finally
                {
                    this.connectionLock.Release();
                }
            }
        }

        private async Task HandleRegularMessage(MsmqMessage m)
        {
            try
            {
                MsmqLog.ReceivedFromChannel(this.logger, m.ChannelName);

                var invocation = this.protocol.ReadInvocation(m.Body);

                var tasks = new List<Task>(this.connections.Count);

                foreach (var connection in this.connections)
                {
                    if (invocation.ExcludedConnectionIds == null || !invocation.ExcludedConnectionIds.Contains(connection.ConnectionId))
                    {
                        tasks.Add(connection.WriteAsync(invocation.Message).AsTask());
                    }
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MsmqLog.FailedWritingMessage(this.logger, ex);
            }
        }

        private async Task HandleGroupMessage(MsmqMessage m)
        {
            try
            {
                var groupMessage = this.protocol.ReadGroupCommand(m.Body);

                var connection = this.connections[groupMessage.ConnectionId];
                if (connection == null)
                {
                    // user not on this server
                    return;
                }

                if (groupMessage.Action == GroupAction.Remove)
                {
                    await this.RemoveGroupAsyncCore(connection, groupMessage.GroupName);
                }

                if (groupMessage.Action == GroupAction.Add)
                {
                    await this.AddGroupAsyncCore(connection, groupMessage.GroupName);
                }

                // Send an ack to the server that sent the original command.
                var ack = this.protocol.WriteAck(groupMessage.Id);
                await this.msmqBus.PublishAsync(this.queues.GroupManagement(groupMessage.ServerName), ack);
            }
            catch (Exception ex)
            {
                MsmqLog.InternalMessageFailed(this.logger, ex);
            }
        }

        private Task HandleAckMessage(MsmqMessage m)
        {
            try
            {
                var ackId = this.protocol.ReadAck(m.Body);
                this.ackHandler.TriggerAck(ackId);
            }
            catch (Exception ex)
            {
                MsmqLog.InternalMessageFailed(this.logger, ex);
            }

            return Task.CompletedTask;
        }
    }
}
