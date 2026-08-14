using Microsoft.EntityFrameworkCore;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data;

public class BgDbContext : DbContext
{
    public DbSet<BoardGame> BoardGames => Set<BoardGame>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Domain> Domains => Set<Domain>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Mechanic> Mechanics => Set<Mechanic>();
    public DbSet<BoardGames_Domains> BoardGames_Domains => Set<BoardGames_Domains>();
    public DbSet<BoardGames_Mechanics> BoardGames_Mechanics => Set<BoardGames_Mechanics>();

    public BgDbContext(DbContextOptions<BgDbContext> options)
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
