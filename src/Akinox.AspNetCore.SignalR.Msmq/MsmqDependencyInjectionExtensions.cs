using System;
using Akinox.AspNetCore.SignalR.Msmq;
using Microsoft.AspNetCore.SignalR;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring Msmq-based scale-out for a SignalR Server in an <see cref="ISignalRServerBuilder" />.
    /// </summary>
    public static class MsmqDependencyInjectionExtensions
    {
        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Msmq server.
        /// </summary>
        /// <param name="signalrBuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static ISignalRServerBuilder AddSignalRMsmq(this ISignalRServerBuilder signalrBuilder)
        {
            return AddSignalRMsmq(signalrBuilder, o => { });
        }

        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Msmq server.
        /// </summary>
        /// <param name="signalrBuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <param name="msmqConnectionString">The connection string used to connect to the Msmq server.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static ISignalRServerBuilder AddSignalRMsmq(this ISignalRServerBuilder signalrBuilder, string msmqConnectionString)
        {
            return AddSignalRMsmq(signalrBuilder, o =>
            {
                o.ConnectionString = msmqConnectionString;
            });
        }

        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Msmq server.
        /// </summary>
        /// <param name="signalrBuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <param name="configure">A callback to configure the Msmq options.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static ISignalRServerBuilder AddSignalRMsmq(this ISignalRServerBuilder signalrBuilder, Action<MsmqOptions> configure)
        {
            signalrBuilder.Services.Configure(configure);
            signalrBuilder.Services.AddSingleton(typeof(HubLifetimeManager<>), typeof(MsmqHubLifetimeManager<>));
            return signalrBuilder;
        }

        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Msmq server.
        /// </summary>
        /// <param name="signalrBuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <param name="msmqConnectionString">The connection string used to connect to the Msmq server.</param>
        /// <param name="configure">A callback to configure the Msmq options.</param>
        /// <returns>The same instance of the <see cref="ISignalRServerBuilder"/> for chaining.</returns>
        public static ISignalRServerBuilder AddSignalRMsmq(this ISignalRServerBuilder signalrBuilder, string msmqConnectionString, Action<MsmqOptions> configure)
        {
            return AddSignalRMsmq(signalrBuilder, o =>
            {
                o.ConnectionString = msmqConnectionString;
                configure(o);
            });
        }
    }
}
