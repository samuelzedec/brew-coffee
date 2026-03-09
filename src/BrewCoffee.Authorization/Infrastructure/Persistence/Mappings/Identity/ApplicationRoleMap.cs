using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Identity;

internal sealed class ApplicationRoleMap
    : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("role");

        builder
            .HasKey(u => u.Id)
            .HasName("pk_role_id");

        builder
            .Property(u => u.Id)
            .HasColumnType("uuid")
            .HasColumnName("id")
            .IsRequired();

        builder
            .Property(r => r.Name)
            .HasColumnType("text")
            .HasColumnName("name")
            .IsRequired();

        builder
            .Property(r => r.NormalizedName)
            .HasColumnType("text")
            .HasColumnName("normalized_name")
            .IsRequired();

        builder
            .HasIndex(r => r.NormalizedName)
            .HasDatabaseName("ix_role_normalized_name")
            .IsUnique();

        builder
            .Property(r => r.ConcurrencyStamp)
            .HasColumnType("text")
            .HasColumnName("concurrency_stamp")
            .IsConcurrencyToken();
    }
}