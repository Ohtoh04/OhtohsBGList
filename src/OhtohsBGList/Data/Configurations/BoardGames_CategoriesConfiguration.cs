using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data.Configurations;

public class BoardGames_CategoriesConfiguration : IEntityTypeConfiguration<BoardGames_Categories>
{
    public void Configure(EntityTypeBuilder<BoardGames_Categories> builder)
    {
        builder.ToTable("BoardGames_Categories");

        builder.HasKey(x => new { x.BoardGameId, x.CategoryId });

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.HasOne(x => x.BoardGame)
            .WithMany(x => x.BoardGames_Categories)
            .HasForeignKey(x => x.BoardGameId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.BoardGames_Categories)
            .HasForeignKey(x => x.CategoryId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
