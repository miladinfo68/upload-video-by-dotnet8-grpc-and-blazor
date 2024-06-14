using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Shared.Protos;

namespace ConsoleAppAsServer.Services;
public class WelcomeServiceImpl : WelcomeService.WelcomeServiceBase
{
    public override Task<WelcomeResponse> Welcome(WelcomeRequest request, ServerCallContext context)
    {
        var message = $"Hello {request.FirstName} {request.LastName} welcome to you!";
        return Task.FromResult(new WelcomeResponse { Message = message });
    }

    public override async Task SendNotification(
        IAsyncStreamReader<NotificationsRequest> requestStream,
        IServerStreamWriter<NotificationsResponse> responseStream,
        ServerCallContext context)
    {
        while (await requestStream.MoveNext())
        {
            var notifReq = requestStream.Current;
            var now = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow);
            var reply = new NotificationsResponse
            {
                Message = $"Hi {notifReq.From}!, I got your message [xxxx {notifReq.Message} to {notifReq.To} xxxx]",
                ReceivedAt = now
            };
            await responseStream.WriteAsync(reply);
        }
    }
}
