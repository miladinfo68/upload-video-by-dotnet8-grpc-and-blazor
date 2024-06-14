using Bogus;
using Microsoft.EntityFrameworkCore;


namespace ConsoleAppAsServer.Data;

public class ServerAppDbContext : DbContext
{
    public ServerAppDbContext(DbContextOptions<ServerAppDbContext> options) : base(options)
    {
    }
    public DbSet<Person> People { get; set; }
    public void PrintPeople() => People.ToList().ForEach(s =>
        Console.WriteLine($"Id :{s.Id}, FirstName: {s.FirstName}, LastName: {s.LastName}"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var faker = new Faker();
        var fakerList = Enumerable.Range(1, 100).Select(i => new Person
        {
            Id = i,
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
        });
        modelBuilder.Entity<Person>(e => e.HasData(fakerList));
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        optionsBuilder.UseInMemoryDatabase("PeopleDb");
    }
}


