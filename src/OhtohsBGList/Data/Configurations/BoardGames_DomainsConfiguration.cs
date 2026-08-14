using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data.Configurations;

public class BoardGames_DomainsConfiguration : IEntityTypeConfiguration<BoardGames_Domains>
{
    public void Configure(EntityTypeBuilder<BoardGames_Domains> builder)
    {
        builder.ToTable("BoardGames_Domains");

        builder.HasKey(x => new { x.BoardGameId, x.DomainId });

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.HasOne(x => x.BoardGame)
            .WithMany(x => x.BoardGames_Domains)
            .HasForeignKey(x => x.BoardGameId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Domain)
            .WithMany(x => x.BoardGames_Domains)
            .HasForeignKey(x => x.DomainId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
