using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OhtohsBGList.Data.Models;

namespace OhtohsBGList.Data.Configurations;

public class MechanicConfiguration : IEntityTypeConfiguration<Mechanic>
{
    public void Configure(EntityTypeBuilder<Mechanic> builder)
    {
        builder.ToTable("Mechanics");

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

        builder.HasMany(x => x.BoardGames_Mechanics)
            .WithOne(x => x.Mechanic)
            .HasForeignKey(x => x.MechanicId)
            .HasPrincipalKey(x => x.Id);
    }
}
