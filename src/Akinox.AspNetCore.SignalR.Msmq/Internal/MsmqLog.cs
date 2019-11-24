using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Akinox.AspNetCore.SignalR.Msmq.Internal
{
    // We don't want to use our nested static class here because MsmqHubLifetimeManager is generic.
    // We'd end up creating separate instances of all the LoggerMessage.Define values for each Hub.
    internal static class MsmqLog
    {
        private static readonly Action<ILogger, string, string, Exception> ConnectingToEndpointsValue =
            LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(1, "ConnectingToEndpoints"), "Connecting to MSMQ endpoints: {Endpoints}. Using Application Name: {ApplicationName}");

        private static readonly Action<ILogger, Exception> ConnectedValue =
            LoggerMessage.Define(LogLevel.Information, new EventId(2, "Connected"), "Connected to Msmq.");

        private static readonly Action<ILogger, string, Exception> SubscribingValue =
            LoggerMessage.Define<string>(LogLevel.Trace, new EventId(3, "Subscribing"), "Subscribing to channel: {Channel}.");

        private static readonly Action<ILogger, string, Exception> ReceivedFromChannelValue =
            LoggerMessage.Define<string>(LogLevel.Trace, new EventId(4, "ReceivedFromChannel"), "Received message from Msmq channel {Channel}.");

        private static readonly Action<ILogger, string, Exception> PublishToChannelValue =
            LoggerMessage.Define<string>(LogLevel.Trace, new EventId(5, "PublishToChannel"), "Publishing message to Msmq channel {Channel}.");

        private static readonly Action<ILogger, string, Exception> UnsubscribeValue =
            LoggerMessage.Define<string>(LogLevel.Trace, new EventId(6, "Unsubscribe"), "Unsubscribing from channel: {Channel}.");

        private static readonly Action<ILogger, Exception> NotConnectedValue =
            LoggerMessage.Define(LogLevel.Error, new EventId(7, "Connected"), "Not connected to Msmq.");

        private static readonly Action<ILogger, Exception> ConnectionRestoredValue =
            LoggerMessage.Define(LogLevel.Information, new EventId(8, "ConnectionRestored"), "Connection to Msmq restored.");

        private static readonly Action<ILogger, Exception> ConnectionFailedValue =
            LoggerMessage.Define(LogLevel.Error, new EventId(9, "ConnectionFailed"), "Connection to Msmq failed.");

        private static readonly Action<ILogger, Exception> FailedWritingMessageValue =
            LoggerMessage.Define(LogLevel.Debug, new EventId(10, "FailedWritingMessage"), "Failed writing message.");

        private static readonly Action<ILogger, Exception> InternalMessageFailedValue =
            LoggerMessage.Define(LogLevel.Warning, new EventId(11, "InternalMessageFailed"), "Error processing message for internal server message.");

        public static void ConnectingToEndpoints(ILogger logger, string connectionString, string serverName)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                ConnectingToEndpointsValue(logger, connectionString, serverName, null);
            }
        }

        public static void Connected(ILogger logger)
        {
            ConnectedValue(logger, null);
        }

        public static void Subscribing(ILogger logger, string channelName)
        {
            SubscribingValue(logger, channelName, null);
        }

        public static void ReceivedFromChannel(ILogger logger, string channelName)
        {
            ReceivedFromChannelValue(logger, channelName, null);
        }

        public static void PublishToChannel(ILogger logger, string channelName)
        {
            PublishToChannelValue(logger, channelName, null);
        }

        public static void Unsubscribe(ILogger logger, string channelName)
        {
            UnsubscribeValue(logger, channelName, null);
        }

        public static void NotConnected(ILogger logger)
        {
            NotConnectedValue(logger, null);
        }

        public static void ConnectionRestored(ILogger logger)
        {
            ConnectionRestoredValue(logger, null);
        }

        public static void ConnectionFailed(ILogger logger, Exception exception)
        {
            ConnectionFailedValue(logger, exception);
        }

        public static void FailedWritingMessage(ILogger logger, Exception exception)
        {
            FailedWritingMessageValue(logger, exception);
        }

        public static void InternalMessageFailed(ILogger logger, Exception exception)
        {
            InternalMessageFailedValue(logger, exception);
        }

        // This isn't DefineMessage-based because it's just the simple TextWriter logging from ConnectionMultiplexer
        public static void ConnectionMultiplexerMessage(ILogger logger, string message)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                // We tag it with EventId 100 though so it can be pulled out of logs easily.
                logger.LogDebug(new EventId(100, "MsmqConnectionLog"), message);
            }
        }
    }
}
