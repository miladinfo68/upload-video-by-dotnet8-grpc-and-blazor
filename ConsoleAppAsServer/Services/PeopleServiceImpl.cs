using Grpc.Core;
using ConsoleAppAsServer.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Protos;

namespace ConsoleAppAsServer.Services;
public class PeopleServiceImpl : PeopleService.PeopleServiceBase
{
    private readonly ServerAppDbContext _cntx;

    public PeopleServiceImpl(ServerAppDbContext cntx)
    {
        _cntx = cntx;
    }

    public override async Task<CreatePersonResponse> Create(
        CreatePersonRequest request, ServerCallContext context)
    {
        var newPerson = new Person
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Childrens = request.Children.Select(s => s.FirstName).ToList(),
        };
        _cntx.People.Add(newPerson);
        await _cntx.SaveChangesAsync(CancellationToken.None);
        return new CreatePersonResponse()
        {
            Id = newPerson.Id,
            FirstName = newPerson.FirstName,
            LastName = newPerson.LastName,
            Childs = string.Join(',', newPerson.Childrens)
        };
    }

    public override async Task<GetAllPeopleResponse> GetAllPeoples(Empty request, ServerCallContext context)
    {
        var dbpeopls = await _cntx.People.ToListAsync();
        var response = new GetAllPeopleResponse();
        response.Peoples.AddRange(dbpeopls.ConvertAll(m => new PeopleModel
        {
            Id = m.Id,
            FirstName = m.FirstName,
            LastName = m.LastName
        }));

        return response;
    }


}
