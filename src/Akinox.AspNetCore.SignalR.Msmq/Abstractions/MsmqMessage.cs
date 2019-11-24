using System;
using System.Collections.Generic;
using System.Text;

namespace Akinox.AspNetCore.SignalR.Msmq.Abstractions
{
    public struct MsmqMessage
    {
        public string ChannelName { get; set; }

        public byte[] Body { get; private set; }

        public MsmqMessage(string channelName, byte[] body)
        {
            this.ChannelName = channelName;
            this.Body = body;
        }
    }
}
