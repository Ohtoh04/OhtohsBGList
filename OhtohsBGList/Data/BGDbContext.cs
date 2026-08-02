using Microsoft.EntityFrameworkCore;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data;

public class BGDbContext : DbContext
{
    public DbSet<BoardGame> BoardGames => Set<BoardGame>();

    public BGDbContext(DbContextOptions<BGDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
