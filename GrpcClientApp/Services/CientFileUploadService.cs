using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Shared.Protos;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GrpcClientApp.Services;

public class CientFileUploadService
{
    private readonly GrpcChannel _channel;
    private readonly UploaderService.UploaderServiceClient _client;
    private ConcurrentDictionary<string, ChunkRequest> _crashedChunks;
    public CientFileUploadService(GrpcChannel channel)
    {
        _channel ??= channel;
        _crashedChunks ??= new();
        _client ??= new UploaderService.UploaderServiceClient(channel);
    }

    public async Task UploadFileAsync(Stream fileStream, int chunkSize, string filePath, Action<int>? callback)
    {
        var totalFileSize = (int)fileStream.Length;
        var totalChunks = (int)Math.Ceiling((double)totalFileSize / chunkSize);
        var buffer = new byte[chunkSize];
        var fileName = filePath.Split('\\').Last();

        using var call = _client.UploadChunk();

        for (var chunkNumber = 1; chunkNumber <= totalChunks; chunkNumber++)
        {
            var bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
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
        await ResendCrashedChunksAsync();
    }


    private async Task ResendCrashedChunksAsync()
    {
        foreach (var crashedChunk in _crashedChunks)
        {
            var chunkNumber = crashedChunk.Key;
            var chunkData = crashedChunk.Value;

            // Create a new call for each chunk
            using var call = _client.UploadChunk();

            try
            {
                await call.RequestStream.WriteAsync(new ChunkRequest
                {
                    Data = chunkData.,
                    ChunkNumber = chunkNumber,
                    TotalChunks = _crashedChunks.Count, // Update totalChunks based on current count
                    FileName = _crashedChunks.First().Value.FileName // Assuming all chunks have the same file name
                });

                // Optionally, wait for a response or check success
            }
            catch (RpcException ex)
            {
                // Log or handle the exception as needed
                Console.WriteLine($"Failed to resend chunk {chunkNumber}: {ex.Status.Detail}");
            }
        }
    }


}