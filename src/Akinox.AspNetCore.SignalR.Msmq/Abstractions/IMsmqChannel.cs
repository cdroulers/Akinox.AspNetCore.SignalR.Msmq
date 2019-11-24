using System;
using System.Threading.Tasks;

namespace Akinox.AspNetCore.SignalR.Msmq.Abstractions
{
    public interface IMsmqChannel
    {
        public void OnMessage(Action<MsmqMessage> handler);

        public void OnMessage(Func<MsmqMessage, Task> handler);

        public void Unsubscribe();

        public Task UnsubscribeAsync();
    }
}
