using Grpc.Core;
using Shared.Protos;
using System.Text;

namespace ConsoleAppAsClient.Services;

public static class ClientMathService
{
    public static async Task Factorial_ServerStreaming(Channel channel, int number)
    {

        var client = new MathService.MathServiceClient(channel);
        using var call = client.Factorial_ServerStreaming(new FactorialRequest { Number = number });
        //1
        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"Factorial of {response.Number} is {response.Result}");
        }

        //2
        //while (await call.ResponseStream.MoveNext())
        //{
        //    var current= call.ResponseStream.Current;
        //    Console.WriteLine($"Factorial of {current.Number} is {current.Result}");
        //}
    }

    public static async Task Average_ClientStreaming(Channel channel)
    {
        var client = new MathService.MathServiceClient(channel);
        using var call = client.Average_ClientStreaming();
        Console.WriteLine("Enter some numbers to get average (enter 'q' to stop):");
        var userNumbers = new List<int>();
        string number;
        while ((number = Console.ReadLine()) != "q")
        {
            if (int.TryParse(number, out var num))
            {
                userNumbers.Add(num);
                await call.RequestStream.WriteAsync(new AverageNumbersRequest { Number = num });
            }
            else
            {
                Console.WriteLine("Invalid number entered. Please try again.");
            }
        }
        await call.RequestStream.CompleteAsync();
        var response = await call.ResponseAsync;
        Console.WriteLine($"The average of the numbers [{string.Join(", ", userNumbers)}] is: {response.Result}");
    }


    public static async Task Sum_ClientServerBothStreaming(Channel channel)
    {
        var client = new MathService.MathServiceClient(channel);
        using var call = client.Sum_ClientServerBothStreaming();
        Console.WriteLine("Enter some numbers to get sum of them (enter 'q' to stop):");
        var userNumbers = new List<int>();
        string number;


        //get response as stream
        var responseReaderTask = Task.Run(async () =>
        {
            while (await call.ResponseStream.MoveNext())
            {
                var response = call.ResponseStream.Current;
                Console.WriteLine($"The average of the numbers [{string.Join(", ", userNumbers)}] is: {response.Result}");
            }
        });


        while ((number = Console.ReadLine()) != "q")
        {
            if (int.TryParse(number, out var num))
            {
                userNumbers.Add(num);
                await call.RequestStream.WriteAsync(new SumNumbersRequest() { Number = num });
            }
            else
            {
                Console.WriteLine("Invalid number entered. Please try again.");
            }
        }
        await call.RequestStream.CompleteAsync();
        await responseReaderTask;

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();


        //await foreach (var response in call.ResponseStream.ReadAllAsync())
        //{
        //    Console.WriteLine($"The sum of the numbers [{string.Join(", ", userNumbers)}] is: {response.Result}");
        //}

    }
}



