using Grpc.Core;
using Shared.Protos;

namespace ConsoleAppAsServer.Services;

public class UploaderServiceImpl : UploaderService.UploaderServiceBase
{
    public override Task UploadFile(
        IAsyncStreamReader<FileChunk> requestStream, 
        IServerStreamWriter<UploadProgress> responseStream, 
        ServerCallContext context)
    {
        return base.UploadFile(requestStream, responseStream, context);
    }
}
