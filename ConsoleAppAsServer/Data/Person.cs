using Bogus;

namespace ConsoleAppAsServer.Data;
public class Person
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public List<string>? Childrens { get; set; } = new();
}


public class PersonFaker : Faker<Person>
{
    int id = 1;
    public PersonFaker()
    {
        RuleFor(p => p.Id, _ => id++);
        RuleFor(p => p.FirstName, f => f.Name.FirstName());
        RuleFor(p => p.LastName, f => f.Name.LastName());
    }

}


