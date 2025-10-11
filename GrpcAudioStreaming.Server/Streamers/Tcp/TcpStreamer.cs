using GrpcAudioStreaming.Proto.Codecs;
using GrpcAudioStreaming.Server.Services.Recorders;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcAudioStreaming.Server.Streamers.Tcp
{
    public class TcpStreamer : IHostedService, IDisposable
    {
        private readonly ILoopbackAudioStreamerService _audioStreamerService;
        private readonly TcpListener _tcpListener;
        private Task _tcpTask;

        public TcpStreamer(ILoopbackAudioStreamerService audioStreamerService)
        {
            _audioStreamerService = audioStreamerService;
            _tcpListener = new TcpListener(System.Net.IPAddress.Any, 8001);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _tcpListener.Start();
            _tcpTask = AcceptClientsAsync(cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            _tcpTask?.Dispose();

            return Task.CompletedTask;
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
                    _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using var networkStream = client.GetStream();

                _audioStreamerService.SetWaveFormat(48000, 16, 2);
                _audioStreamerService.SetCodec(CodecFactory.GetOrDefault(Codecs.Pcm));

                _audioStreamerService.RegisterNewConsumer(new Models.AudioConsumer(client.Client.RemoteEndPoint.ToString(), client.Client.RemoteEndPoint.ToString()));

                await foreach (var (data, _) in _audioStreamerService.Source.GetAsyncEnumerable(cancellationToken))
                {
                    await networkStream.WriteAsync(data, cancellationToken);
                }

                // Keep the connection open
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        public void Dispose()
        {
            _tcpListener?.Stop();
            _audioStreamerService?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
