# WASAPI loopback audio streaming via gRPC

Uses [NAudio](https://github.com/naudio/NAudio) to capture live audio data and transmit it to a client via gRPC with low latency.

Supported codec
- `Pcm` (LPCM) (raw data)
- `Mulaw` (G.711 mu-law)

Requires .NET 8

Only supports Windows.

------

Inspired by https://github.com/schneider-m/GrpcAudioStreaming
