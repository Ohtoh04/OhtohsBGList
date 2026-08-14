using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data.Configurations;

public class BoardGameConfiguration : IEntityTypeConfiguration<BoardGame>
{
    public void Configure(EntityTypeBuilder<BoardGame> builder)
    {
        builder.ToTable("BoardGames");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.AlternateNames)
            .HasMaxLength(200);

        builder.Property(x => x.Designer)
            .HasMaxLength(200);

        builder.Property(x => x.Flags)
            .IsRequired();

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.MinPlayers)
            .IsRequired();

        builder.Property(x => x.MaxPlayers)
            .IsRequired();

        builder.Property(x => x.PlayTime)
            .IsRequired();

        builder.Property(x => x.MinAge)
            .IsRequired();

        builder.Property(x => x.UsersRated)
            .IsRequired();

        builder.Property(x => x.RatingAverage)
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(x => x.BGGRank)
            .IsRequired();

        builder.Property(x => x.ComplexityAverage)
            .HasPrecision(4, 2)
            .IsRequired();

        builder.Property(x => x.OwnedUsers)
            .IsRequired();

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.LastModifiedDate)
            .IsRequired();

        builder.HasMany(x => x.BoardGames_Domains)
            .WithOne(x => x.BoardGame)
            .HasForeignKey(x => x.BoardGameId)
            .HasPrincipalKey(x => x.Id);

        builder.HasMany(x => x.BoardGames_Mechanics)
            .WithOne(x => x.BoardGame)
            .HasForeignKey(x => x.BoardGameId)
            .HasPrincipalKey(x => x.Id);

        builder.HasMany(x => x.BoardGames_Categories)
            .WithOne(x => x.BoardGame)
            .HasForeignKey(x => x.BoardGameId)
            .HasPrincipalKey(x => x.Id);

        builder.HasOne(x => x.Publisher)
            .WithMany(x => x.BoardGames)
            .HasForeignKey(x => x.PublisherId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
