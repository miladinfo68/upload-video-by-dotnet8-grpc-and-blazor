using Grpc.Core;
using Shared.Protos;


namespace GrpcServerApp.Services;

public class VideoServiceImpl : VideoService.VideoServiceBase
{
    private readonly IWebHostEnvironment _WebHostEnvironment;
    private readonly string _rootPath;

    public VideoServiceImpl(IWebHostEnvironment webHostEnvironment)
    {
        _WebHostEnvironment = webHostEnvironment;
        _rootPath = AppDomain.CurrentDomain.BaseDirectory;
    }


    public override async Task UploadChunk(
        IAsyncStreamReader<ChunkRequest> requestStream,
        IServerStreamWriter<ChunkResponse> responseStream,
        ServerCallContext context)
    {

        var lastProcessedChunkNumber = 0;

        while (await requestStream.MoveNext())
        {
            var chunkInfo = requestStream.Current;

            var (hasSaved, message) = SaveChunk(chunkInfo);
            var response = new ChunkResponse()
            {
                Message = message
            };
            if (hasSaved)
            {
                response.Success = true;
                response.Progress = CalculateProgress(chunkInfo.TotalChunks, ++lastProcessedChunkNumber);
                response.FailedChunkNumber = null;
            }
            else
            {
                response.Success = false;
                response.Progress = lastProcessedChunkNumber > 0 ?
                    CalculateProgress(chunkInfo.TotalChunks, lastProcessedChunkNumber) : 0;
                response.FailedChunkNumber = chunkInfo.ChunkNumber;
                response.FailedChunkData = chunkInfo.Data;
            }
            await responseStream.WriteAsync(response);

        }
    }

    private int CalculateProgress(int totalChunks, int receivedChunk)
        => (int)(((double)receivedChunk / totalChunks) * 100);


    //private static bool SaveChunk(ByteString data)
    private (bool, string) SaveChunk(ChunkRequest chunk)
    {
        var res = true;
        var message = string.Empty;
        string projectRootPath = Path.GetFullPath(Path.Combine(_rootPath, @"..\..\.."));
        string videosFolderPath = Path.Combine(projectRootPath, "Videos");
        //var filePath = Path.Combine(_WebHostEnvironment.WebRootPath, "files");

        if (!Directory.Exists(videosFolderPath))
        {
            Directory.CreateDirectory(videosFolderPath);
        }

        var fileChunksPath = Path.Combine(videosFolderPath, Path.GetFileNameWithoutExtension(chunk.FileName));

        if (!Directory.Exists(fileChunksPath))
        {
            Directory.CreateDirectory(fileChunksPath);
        }
        //var chunkName = $"chunk-{chunk.ChunkNumber.ToString().PadLeft(4, '0')}{Path.GetExtension(chunk.FileName)}";
        var chunkName = $"chunk-{chunk.ChunkNumber:D4}{Path.GetExtension(chunk.FileName)}";
        
        var chunkFileNameFullPath = Path.Combine(fileChunksPath, $"{chunkName}");

        try
        {
            if (!File.Exists(chunkFileNameFullPath))
            {
                File.WriteAllBytes(chunkFileNameFullPath, chunk.Data.ToByteArray());
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
