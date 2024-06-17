using Grpc.Core;
using Shared.Protos;

namespace ConsoleAppAsServer.Services;

public class VideoServiceImpl : VideoService.VideoServiceBase
{
    public override Task UploadChunk(
        IAsyncStreamReader<ChunkRequest> requestStream, 
        IServerStreamWriter<ChunkResponse> responseStream, 
        ServerCallContext context)
    {
        return base.UploadChunk(requestStream, responseStream, context);
    }
}
