using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data.Configurations;

public class DomainConfiguration : IEntityTypeConfiguration<Domain>
{
    public void Configure(EntityTypeBuilder<Domain> builder)
    {
        builder.ToTable("Domains");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(200);

        builder.Property(x => x.Flags)
            .IsRequired();

        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.LastModifiedDate)
            .IsRequired();

        builder.HasMany(x => x.BoardGames_Domains)
            .WithOne(x => x.Domain)
            .HasForeignKey(x => x.DomainId)
            .HasPrincipalKey(x => x.Id);
    }
}
