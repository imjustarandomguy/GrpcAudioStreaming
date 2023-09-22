using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Utils
{
    /// <summary>
    /// Wraps <see cref="IServerStreamWriter{T}"/> which only supports one writer at a time.
    /// This class can receive messages from multiple threads, and writes them to the stream
    /// one at a time.
    /// </summary>
    /// <typeparam name="T">Type of message written to the stream</typeparam>
    public class GrpcStreamResponseQueue<T>
    {
        public Action OnComplete;

        private readonly IServerStreamWriter<T> _stream;
        private readonly Task _consumer;

        private readonly Channel<T> _channel = System.Threading.Channels.Channel.CreateUnbounded<T>(
            new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = true,
            });

        public GrpcStreamResponseQueue(
            IServerStreamWriter<T> stream,
            CancellationToken cancellationToken = default
        )
        {
            _stream = stream;
            _consumer = Consume(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes an item to the channel.
        /// </summary>
        /// <param name="message">The value to write to the channel.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to cancel the write operation.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.ValueTask" /> that represents the asynchronous write operation.</returns>
        public async ValueTask WriteAsync(T message, CancellationToken cancellationToken = default)
        {
            try
            {
                await _channel.Writer.WriteAsync(message, cancellationToken);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Marks the writer as completed, and waits for all writes to complete.
        /// </summary>
        public Task CompleteAsync()
        {
            _channel.Writer.Complete();
            OnComplete();
            return _consumer;
        }

        private async Task Consume(CancellationToken cancellationToken)
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    await _stream.WriteAsync(message, cancellationToken);
                }
                catch (Exception)
                {
                    await CompleteAsync();
                }
            }
        }
    }
}
