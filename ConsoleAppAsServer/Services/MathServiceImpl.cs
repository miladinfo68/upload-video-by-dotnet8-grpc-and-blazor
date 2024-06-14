using Grpc.Core;
using Shared.Protos;

namespace ConsoleAppAsServer.Services;
public class MathServiceImpl : MathService.MathServiceBase
{
    public override async Task Factorial_ServerStreaming(
        FactorialRequest request,
        IServerStreamWriter<FactorialResponse> responseStream,
        ServerCallContext context)
    {
        var fac = 1;
        var num = request?.Number ?? 1;
        for (var n = 1; num > 0 && n <= num; n++)
        {
            fac *= n;
            await responseStream.WriteAsync(new FactorialResponse
            {
                Number = n,
                Result = fac
            });
            await Task.Delay(1000);
        }
    }

    public override async Task<AverageNumbersResponse> Average_ClientStreaming(
        IAsyncStreamReader<AverageNumbersRequest> requestStream,
        ServerCallContext context)
    {
        var total = 0f;
        var count = 0;

        while (await requestStream.MoveNext())
        {
            ++count;
            total += requestStream.Current.Number;
        }
        return new AverageNumbersResponse
        {
            Result = count > 0 ? total / count : 0
        };
    }

    public override async Task Sum_ClientServerBothStreaming(
        IAsyncStreamReader<SumNumbersRequest> requestStream, 
        IServerStreamWriter<SumNumbersResponse> responseStream,
        ServerCallContext context)
    {
        var total = 0f;
        while (await requestStream.MoveNext())
        {
            total += requestStream.Current.Number;
            await responseStream.WriteAsync(new SumNumbersResponse
            {
                Result = total
            });
        }
    }
}
