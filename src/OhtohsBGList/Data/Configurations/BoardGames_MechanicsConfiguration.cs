using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data.Configurations;

public class BoardGames_MechanicsConfiguration : IEntityTypeConfiguration<BoardGames_Mechanics>
{
    public void Configure(EntityTypeBuilder<BoardGames_Mechanics> builder)
    {
        builder.ToTable("BoardGames_Mechanics");

        builder.HasKey(x => new { x.BoardGameId, x.MechanicId });

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.HasOne(x => x.BoardGame)
            .WithMany(x => x.BoardGames_Mechanics)
            .HasForeignKey(x => x.BoardGameId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Mechanic)
            .WithMany(x => x.BoardGames_Mechanics)
            .HasForeignKey(x => x.MechanicId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
