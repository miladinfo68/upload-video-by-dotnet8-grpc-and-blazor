using Bogus;
using Grpc.Core;
using ConsoleAppAsServer.Data;
using ConsoleAppAsServer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Protos;
using Shared;


var serviceProvider = ConfigureServices();
var peopleContext = serviceProvider.GetRequiredService<ServerAppDbContext>();
peopleContext.Database.EnsureCreated();


var server = new Server()
{
    Ports = { new ServerPort(AppConstants.SERVER_HOST_NAME, AppConstants.SERVER_LISTEN_PORT, ServerCredentials.Insecure) },
    Services =
    {
        WelcomeService.BindService(new WelcomeServiceImpl()),
        MathService.BindService(new MathServiceImpl()),
        VideoService.BindService(new VideoServiceImpl()),
        PeopleService.BindService(new PeopleServiceImpl(peopleContext)),
    }
};

try
{
    server.Start();
    Console.WriteLine($"[xxx Server] is running on port {AppConstants.SERVER_LISTEN_PORT}");
    
    //peopleContext.PrintPeople();
    //var persons = new PersonFaker().Generate(100);
    
    Console.ReadLine();
}
catch (Exception exp)
{
    Console.WriteLine($"An error has been occured : {exp.Message}");
}
finally
{
    await server.ShutdownAsync();
}



static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();

    services.AddDbContext<ServerAppDbContext>(opt=>opt.UseInMemoryDatabase("PeopleDb"));

    return services.BuildServiceProvider();
}

