using Hamekoz.Api.Example.Models;

using Microsoft.EntityFrameworkCore;

namespace Hamekoz.Api.Example.Data;
public class HamekozExampleDbContext(DbContextOptions<HamekozExampleDbContext> options) : DbContext(options)
{
    public DbSet<Person> People { get; set; }

}
