using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Identity;

internal sealed class ApplicationRoleClaimMap
    : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
        builder.ToTable("role_claim");

        builder
            .HasKey(rc => rc.Id)
            .HasName("pk_role_claim_id");

        builder
            .Property(rc => rc.Id)
            .HasColumnType("integer")
            .HasColumnName("id")
            .UseIdentityColumn();

        builder
            .Property(rc => rc.RoleId)
            .HasColumnType("uuid")
            .HasColumnName("role_id")
            .IsRequired();

        builder
            .Property(rc => rc.ClaimType)
            .HasColumnType("text")
            .HasColumnName("claim_type")
            .IsRequired();

        builder
            .Property(rc => rc.ClaimValue)
            .HasColumnType("text")
            .HasColumnName("claim_value")
            .IsRequired();

        builder
            .HasIndex(rc => rc.RoleId)
            .HasDatabaseName("ix_role_claim_role_id");
    }
}