using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Akinox.AspNetCore.SignalR.Msmq.Abstractions;

namespace Akinox.AspNetCore.SignalR.Msmq.Implementations
{
    public class BasicMsmqChannel : IMsmqChannel
    {
        public void OnMessage(Action<MsmqMessage> handler)
        {
            throw new NotImplementedException();
        }

        public void OnMessage(Func<MsmqMessage, Task> handler)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }

        public Task UnsubscribeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
