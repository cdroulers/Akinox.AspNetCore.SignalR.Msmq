using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Akinox.AspNetCore.SignalR.Msmq
{
    /// <summary>
    /// Options used to configure <see cref="MsmqHubLifetimeManager{THub}"/>.
    /// </summary>
    public class MsmqOptions
    {
        public string ConnectionString { get; set; }
    }
}
