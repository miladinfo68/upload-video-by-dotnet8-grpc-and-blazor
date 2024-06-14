using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.VisualBasic;
using Shared.Protos;
using System.Collections.Immutable;


namespace GrpcServerApp.Services;

public class UploaderServiceImpl : UploaderService.UploaderServiceBase
{
    private readonly IWebHostEnvironment _WebHostEnvironment;

    public UploaderServiceImpl(IWebHostEnvironment webHostEnvironment)
    {
        _WebHostEnvironment = webHostEnvironment;
    }


    public override async Task UploadChunk(
        IAsyncStreamReader<ChunkRequest> requestStream,
        IServerStreamWriter<ChunkResponse> responseStream,
        ServerCallContext context)
    {
        var filePath = Path.Combine(_WebHostEnvironment.WebRootPath, "files");
        var chunkName = string.Empty;

        while (await requestStream.MoveNext())
        {

            var chunkInfo = requestStream.Current;
            chunkName = $"chunk-{chunkInfo.ChunkNumber.ToString().PadLeft(4, '0')}{Path.GetExtension(chunkInfo.FileName)}";
            chunkInfo.FileName = Path.Combine(filePath, chunkInfo.FileName, $"{chunkName}");
            
            var chunkNumber = chunkInfo.ChunkNumber;
            var (hasSaved, message) = SaveChunk(chunkInfo);

            if (!hasSaved)
            {
                chunkNumber = chunkInfo.ChunkNumber - 1;
            }

            var progress = CalculateProgress(chunkInfo.TotalChunks, chunkNumber);

            var response = new ChunkResponse
            {
                Success = hasSaved,
                Message = message,
                Progress = progress,
                ChunkNumber = chunkInfo.ChunkNumber
            };
            await responseStream.WriteAsync(response);

        }
    }

    private static int CalculateProgress(int totalChunks, int receivedChunks)
        => (int)(((double)receivedChunks / totalChunks) * 100);


    //private static bool SaveChunk(ByteString data)
    private static (bool, string) SaveChunk(ChunkRequest chunk)
    {
        var res = true;
        var message = string.Empty;
        try
        {
            if (!File.Exists(chunk.FileName))
            {
                File.WriteAllBytes(chunk.FileName, chunk.Data.ToArray());
            }
        }
        catch (Exception exp)
        {
            res = false;
            message = exp?.Message ?? exp?.InnerException?.Message ?? $"Error in saving Chunk{chunk.ChunkNumber} ";
            //throw;
        }
        return (res, message);
    }


}
