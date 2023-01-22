using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Utils
{
    public class AsyncEnumerableSource<T>
    {
        private Tuple<ImmutableArray<Channel<T>>> _channels = Tuple.Create(ImmutableArray<Channel<T>>.Empty);

        public async IAsyncEnumerable<T> GetAsyncEnumerable([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var channel = Channel.CreateUnbounded<T>();
            ImmutableInterlocked.Update(ref _channels, (x, y) => Tuple.Create(x.Item1.Add(y)), channel);

            try
            {
                await foreach (var item in channel.Reader.ReadAllAsync().ConfigureAwait(false))
                {
                    yield return item;

                    if (cancellationToken.IsCancellationRequested)
                    {
                        continue;
                    }
                }
            }
            finally
            {
                ImmutableInterlocked.Update(ref _channels, (x, y) => Tuple.Create(x.Item1.Remove(y)), channel);
            }
        }

        public void YieldReturn(T value)
        {
            foreach (var channel in Volatile.Read(ref _channels).Item1)
            {
                channel.Writer.TryWrite(value);
            }
        }

        public void Complete()
        {
            foreach (var channel in Volatile.Read(ref _channels).Item1)
            {
                channel.Writer.TryComplete();
            }
        }

        public void Fault(Exception ex)
        {
            foreach (var channel in Volatile.Read(ref _channels).Item1)
            {
                channel.Writer.TryComplete(ex);
            }
        }
    }
}