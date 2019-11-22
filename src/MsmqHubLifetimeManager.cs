using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Internal;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Akinox.AspNetCore.SignalR.Msmq
{
    public class MsmqHubLifetimeManager<THub> : HubLifetimeManager<THub>, IDisposable where THub : Hub
    {
        private readonly HubConnectionStore _connections = new HubConnectionStore();
        private readonly ILogger _logger;
        private readonly MsmqOptions _options;

        public MsmqHubLifetimeManager(ILogger<MsmqHubLifetimeManager<THub>> logger,
                                       IOptions<MsmqOptions> options,
                                       IHubProtocolResolver hubProtocolResolver)
            : this(logger, options, hubProtocolResolver, globalHubOptions: null, hubOptions: null)
        {
        }

        public MsmqHubLifetimeManager(ILogger<MsmqHubLifetimeManager<THub>> logger,
                                       IOptions<MsmqOptions> options,
                                       IHubProtocolResolver hubProtocolResolver,
                                       IOptions<HubOptions> globalHubOptions,
                                       IOptions<HubOptions<THub>> hubOptions)
        {
            _logger = logger;
            _options = options.Value;
        }
    }
}
