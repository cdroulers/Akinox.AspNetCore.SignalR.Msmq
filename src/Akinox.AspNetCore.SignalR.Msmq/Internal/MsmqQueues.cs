using System;
using System.Collections.Generic;
using System.Text;

namespace Akinox.AspNetCore.SignalR.Msmq.Internal
{
    public class MsmqQueues
    {
        private readonly string applicationName;

        public MsmqQueues(string applicationName)
        {
            this.applicationName = applicationName;
        }

        public string Ack(string serverName)
        {
            return this.applicationName + ":ack:" + serverName;
        }

        public string GroupManagement(string serverName)
        {
            return this.applicationName + ":groups:" + serverName;
        }

        public string Invocations(string serverName)
        {
            return this.applicationName + ":invocations:" + serverName;
        }
    }
}
