using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Identity;

internal sealed class ApplicationUserRoleMap : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        builder.ToTable("user_role");

        builder
            .HasKey(ur => new { ur.UserId, ur.RoleId })
            .HasName("pk_user_role_user_id_role_id");

        builder
            .Property(ur => ur.UserId)
            .HasColumnType("uuid")
            .HasColumnName("user_id")
            .IsRequired();

        builder
            .Property(ur => ur.RoleId)
            .HasColumnType("uuid")
            .HasColumnName("role_id")
            .IsRequired();

        builder
            .HasIndex(ur => ur.UserId)
            .HasDatabaseName("ix_user_role_user_id");

        builder
            .HasIndex(ur => ur.RoleId)
            .HasDatabaseName("ix_user_role_role_id");
    }
}