using Grpc.Core;
using Shared.Protos;

namespace ConsoleAppAsClient.Services;
public static class ClientWelcomeService
{
    public static async Task Welcome(Channel channel)
    {

        var response = await new WelcomeService.WelcomeServiceClient(channel).WelcomeAsync(new WelcomeRequest
        {
            FirstName = "Mahdi",
            LastName = "Jalali"
        });
        Console.WriteLine(response.Message);
    }

    public static async Task SendNotification(Channel channel)
    {
        var client = new WelcomeService.WelcomeServiceClient(channel);
        using var call = client.SendNotification();

        var responseReaderTask = Task.Run(async () =>
        {
            while (await call.ResponseStream.MoveNext(CancellationToken.None))
            {
                var result = call.ResponseStream.Current;
                Console.WriteLine($"{result.Message}, received at {result.ReceivedAt}");
            }
        });


        foreach (var msg in new[] { "Tom", "Jones" })
        {
            var request = new NotificationsRequest()
            {
                Message = $"Hello {msg}",
                From = "Mom",
                To = msg
            };
            await call.RequestStream.WriteAsync(request);
        }

        await call.RequestStream.CompleteAsync();
        await responseReaderTask;

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}


