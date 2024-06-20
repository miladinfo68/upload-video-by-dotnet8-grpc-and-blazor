using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Shared.Protos;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GrpcClientApp.Services;

public class ClientVideoService
{
    private readonly GrpcChannel _channel;
    private readonly VideoService.VideoServiceClient _client;
    private readonly ConcurrentDictionary<string, ChunkRequest> _crashedChunks;
    public ClientVideoService(GrpcChannel channel)
    {
        _channel ??= channel;
        _crashedChunks ??= new();
        _client ??= new VideoService.VideoServiceClient(channel);
    }

    public async Task UploadFileAsync(Stream fileStream, int chunkSize, string filePath, Action<int>? callback)
    {
        var totalFileSize = (int)fileStream.Length;
        var totalChunks = (int)Math.Ceiling((double)totalFileSize / chunkSize);
        int lastChunkSize = (int)(totalFileSize % chunkSize);
        byte[]? buffer = null;
        var fileName = filePath.Split('\\').Last();

        var offset = 0;
        var countBtyesToRead = 0;

        using var call = _client.UploadChunk();

        for (var chunkNumber = 1; chunkNumber <= totalChunks; chunkNumber++)
        {
            offset = (chunkNumber - 1) * chunkSize;
            countBtyesToRead = chunkNumber == totalChunks ? lastChunkSize :  chunkSize;

            // Adjust buffer size for the last chunk
            buffer = new byte[chunkSize];
            var bytesRead = await fileStream.ReadAsync(buffer);
            //var bytesRead = await fileStream.ReadAsync(buffer, offset, countBtyesToRead);
            if (bytesRead == 0) break; // End of file

            var chunk = new ChunkRequest
            {
                Data = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead),
                ChunkNumber = chunkNumber,
                TotalChunks = totalChunks,
                FileName = fileName
            };

            try
            {
                await call.RequestStream.WriteAsync(chunk);

                // Read the response after writing the chunk
                if (!await call.ResponseStream.MoveNext()) continue;
                var response = call.ResponseStream.Current;
                if (response is not null && response.Success)
                {
                    callback?.Invoke(response.Progress);
                }
            }
            catch (RpcException ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Failed to upload chunk {chunkNumber}: {ex.Status.Detail}");

                // Add the chunk to the crashed chunks dictionary
                _crashedChunks.TryAdd($"{chunk.FileName}-{chunk.ChunkNumber}", chunk);
            }


        }

        await call.RequestStream.CompleteAsync();

        // Attempt to resend crashed chunks
        if (!_crashedChunks.IsEmpty)
        {
            await ResendCrashedChunksAsync(callback);
        }

    }


    private async Task ResendCrashedChunksAsync(Action<int>? callback)
    {
        foreach (var crashedChunk in _crashedChunks)
        {
            var chunkInfo = crashedChunk.Value;

            // Create a new call for each chunk
            using var call = _client.UploadChunk();

            try
            {
                await call.RequestStream.WriteAsync(new ChunkRequest
                {
                    Data = chunkInfo.Data,
                    ChunkNumber = chunkInfo.ChunkNumber,
                    TotalChunks = chunkInfo.TotalChunks,
                    FileName = chunkInfo.FileName
                });

                // Read the response after writing the chunk
                if (!await call.ResponseStream.MoveNext()) continue;
                var response = call.ResponseStream.Current;
                if (response is not null && response.Success)
                {
                    callback?.Invoke(response.Progress);
                }
            }
            catch (RpcException ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Failed to resend chunk {chunkInfo.ChunkNumber}: {ex.Status.Detail}");
            }
        }
    }


}