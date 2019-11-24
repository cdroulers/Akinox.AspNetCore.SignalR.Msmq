using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Akinox.AspNetCore.SignalR.Msmq.Internal
{
    internal class AckHandler : IDisposable
    {
        private readonly ConcurrentDictionary<int, AckInfo> acks = new ConcurrentDictionary<int, AckInfo>();
        private readonly Timer timer;
        private readonly TimeSpan ackThreshold = TimeSpan.FromSeconds(30);
        private readonly TimeSpan ackInterval = TimeSpan.FromSeconds(5);
        private readonly object @lock = new object();
        private bool disposed;

        public AckHandler()
        {
            // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
            bool restoreFlow = false;
            try
            {
                if (!ExecutionContext.IsFlowSuppressed())
                {
                    ExecutionContext.SuppressFlow();
                    restoreFlow = true;
                }

                this.timer = new Timer(state => ((AckHandler)state).CheckAcks(), state: this, dueTime: this.ackInterval, period: this.ackInterval);
            }
            finally
            {
                // Restore the current ExecutionContext
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }

        public Task CreateAck(int id)
        {
            lock (this.@lock)
            {
                if (this.disposed)
                {
                    return Task.CompletedTask;
                }

                return this.acks.GetOrAdd(id, _ => new AckInfo()).Tcs.Task;
            }
        }

        public void TriggerAck(int id)
        {
            if (this.acks.TryRemove(id, out var ack))
            {
                ack.Tcs.TrySetResult(null);
            }
        }

        private void CheckAcks()
        {
            if (this.disposed)
            {
                return;
            }

            var utcNow = DateTime.UtcNow;

            foreach (var pair in this.acks)
            {
                var elapsed = utcNow - pair.Value.Created;
                if (elapsed > this.ackThreshold)
                {
                    if (this.acks.TryRemove(pair.Key, out var ack))
                    {
                        ack.Tcs.TrySetCanceled();
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (this.@lock)
            {
                this.disposed = true;

                this.timer.Dispose();

                foreach (var pair in this.acks)
                {
                    if (this.acks.TryRemove(pair.Key, out var ack))
                    {
                        ack.Tcs.TrySetCanceled();
                    }
                }
            }
        }

        private class AckInfo
        {
            public TaskCompletionSource<object> Tcs { get; private set; }

            public DateTime Created { get; private set; }

            public AckInfo()
            {
                this.Created = DateTime.UtcNow;
                this.Tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }
    }
}
