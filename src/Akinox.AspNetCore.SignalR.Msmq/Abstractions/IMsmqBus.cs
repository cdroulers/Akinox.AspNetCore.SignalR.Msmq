using System.Threading.Tasks;

namespace Akinox.AspNetCore.SignalR.Msmq.Abstractions
{
    public interface IMsmqBus
    {
        Task PublishAsync(string queueName, byte[] message);

        Task<string[]> GetAllQueueNames();

        Task<IMsmqChannel> SubscribeAsync(string queueName);
    }
}
