using Grpc.Core;
using ConsoleAppAsClient.Services;
using Shared;


var chanel = new Channel(AppConstants.SERVER_HOST_NAME, AppConstants.SERVER_LISTEN_PORT, ChannelCredentials.Insecure);

try
{
    await chanel.ConnectAsync();
    Console.WriteLine($"[xxx Client] connected to server running on port {AppConstants.SERVER_LISTEN_PORT}");

    //await ClientWelcomeService.Welcome(chanel);
    //await ClientPeopleService.CreatePerson(chanel);
    //await ClientPeopleService.GetAllPeople(chanel);
    //await ClientMathService.Factorial_ServerStreaming(chanel,10);
    await ClientMathService.Average_ClientStreaming(chanel);
    //await ClientMathService.Sum_ClientServerBothStreaming(chanel);
    //await ClientWelcomeService.SendNotification(chanel);

    Console.ReadLine();
}
catch (Exception exp)
{
    Console.WriteLine($"An error has been occured : {exp.Message}");
}
finally
{
    await chanel.ShutdownAsync();
}


