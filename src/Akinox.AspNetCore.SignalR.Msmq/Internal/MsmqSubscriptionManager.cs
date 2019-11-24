using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Akinox.AspNetCore.SignalR.Msmq.Internal
{
    public class MsmqSubscriptionManager
    {
        private readonly ConcurrentDictionary<string, HubConnectionStore> subscriptions = new ConcurrentDictionary<string, HubConnectionStore>(StringComparer.Ordinal);
        private readonly SemaphoreSlim subscriptionLock = new SemaphoreSlim(1, 1);

        public async Task AddSubscriptionAsync(string id, HubConnectionContext connection, Func<string, HubConnectionStore, Task> subscribeMethod)
        {
            await this.subscriptionLock.WaitAsync();

            try
            {
                var subscription = this.subscriptions.GetOrAdd(id, _ => new HubConnectionStore());

                subscription.Add(connection);

                // Subscribe once
                if (subscription.Count == 1)
                {
                    await subscribeMethod(id, subscription);
                }
            }
            finally
            {
                this.subscriptionLock.Release();
            }
        }

        public async Task RemoveSubscriptionAsync(string id, HubConnectionContext connection, Func<string, Task> unsubscribeMethod)
        {
            await this.subscriptionLock.WaitAsync();

            try
            {
                if (!this.subscriptions.TryGetValue(id, out var subscription))
                {
                    return;
                }

                subscription.Remove(connection);

                if (subscription.Count == 0)
                {
                    this.subscriptions.TryRemove(id, out _);
                    await unsubscribeMethod(id);
                }
            }
            finally
            {
                this.subscriptionLock.Release();
            }
        }
    }
}
