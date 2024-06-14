using Grpc.Core;
using Shared.Protos;

namespace ConsoleAppAsClient.Services;

public static class ClientPeopleService
{
    public static async Task CreatePerson(Channel channel)
    {

        Console.WriteLine("Enter First Name : ");
        var firstName = Console.ReadLine();
        Console.WriteLine("Enter Last Name : ");
        var lastName = Console.ReadLine();

        var createNewPerson = new CreatePersonRequest
        {
            FirstName = firstName,
            LastName = lastName,
        };
        createNewPerson.Children.AddRange(new[]
        {
            new Child{FirstName="John"},
            new Child{FirstName="Smith"},
        });
        var response = await new PeopleService.PeopleServiceClient(channel).CreateAsync(createNewPerson);
        Console.WriteLine($"Id: {response.Id}, FirstName : {response.FirstName}, LastName: {response.LastName} ,Children: {response.Childs}");
    }

    public static async Task GetAllPeople(Channel channel)
    {
        var request = new Empty();
        var response = await new PeopleService.PeopleServiceClient(channel).GetAllPeoplesAsync(request);
        response?.Peoples?.ToList()
            .ForEach(p => Console.WriteLine($"Id: {p.Id}, FirstName : {p.FirstName}, LastName: {p.LastName}"));

    }
}

