using System;
using System.Linq;
using System.Threading.Tasks;
using Akinox.AspNetCore.SignalR.Msmq.Abstractions;
using Experimental.System.Messaging;
using Microsoft.Extensions.Options;

namespace Akinox.AspNetCore.SignalR.Msmq.Implementations
{
    public class BasicMsmqBus : IMsmqBus
    {
        private readonly MsmqOptions options;

        private readonly MsmqConnectionString connectionString;

        public BasicMsmqBus(IOptions<MsmqOptions> options)
        {
            this.options = options.Value;
            this.connectionString = MsmqConnectionString.Parse(this.options.ConnectionString);
        }

        public Task<string[]> GetAllQueueNamesAsync()
        {
            return Task.FromResult(MessageQueue.GetPrivateQueuesByMachine(this.connectionString.Host).Select(x => x.QueueName).ToArray());
        }

        public Task PublishAsync(string queueName, byte[] message)
        {
            var connectionString = this.connectionString.Clone();
            connectionString.QueueName = queueName;
            var path = connectionString.Build();

            MessageQueue.Create(path.Replace("FormatName:", string.Empty), true);

            using var messageQueue = new MessageQueue(path);
            messageQueue.Formatter = new BinaryMessageFormatter();
            messageQueue.Send(message, MessageQueueTransactionType.Single);

            return Task.CompletedTask;
        }

        public Message[] GetMessages(string queueName)
        {
            var connectionString = this.connectionString.Clone();
            connectionString.QueueName = queueName;
            var path = connectionString.Build();

            using var messageQueue = new MessageQueue(path);
            messageQueue.Formatter = new BinaryMessageFormatter();

            var messages = messageQueue.GetAllMessages();

            foreach (var message in messages)
            {
                message.Formatter = new BinaryMessageFormatter();
            }

            return messages;
        }

        public Task<IMsmqChannel> SubscribeAsync(string queueName)
        {
            throw new NotImplementedException();
        }
    }
}
