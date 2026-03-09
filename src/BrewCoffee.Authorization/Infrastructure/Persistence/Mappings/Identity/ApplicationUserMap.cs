using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Identity;

internal sealed class ApplicationUserMap : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("user");

        builder
            .HasKey(u => u.Id)
            .HasName("pk_user_id");

        builder
            .Property(u => u.Id)
            .HasColumnType("uuid")
            .HasColumnName("id")
            .IsRequired();

        builder
            .Property(u => u.UserName)
            .HasColumnType("text")
            .HasColumnName("user_name")
            .IsRequired();
        
        builder
            .HasIndex(u => u.UserName, "uq_user_user_name")
            .IsUnique();

        builder
            .Property(u => u.NormalizedUserName)
            .HasColumnType("text")
            .HasColumnName("normalized_user_name")
            .IsRequired();

        builder
            .HasIndex(u => u.NormalizedUserName, "uq_user_normalized_user_name")
            .IsUnique();

        builder
            .Property(u => u.Email)
            .HasColumnType("text")
            .HasColumnName("email")
            .IsRequired();

        builder
            .HasIndex(u => u.Email, "uq_user_email")
            .IsUnique();

        builder
            .Property(u => u.NormalizedEmail)
            .HasColumnType("text")
            .HasColumnName("normalized_email");
        
        builder
            .HasIndex(u => u.NormalizedEmail, "uq_user_normalized_email")
            .IsUnique();

        builder
            .Property(u => u.EmailConfirmed)
            .HasColumnType("boolean")
            .HasColumnName("email_confirmed")
            .IsRequired();

        builder
            .Property(u => u.PasswordHash)
            .HasColumnType("text")
            .HasColumnName("password_hash")
            .IsRequired(false);

        builder
            .Property(u => u.PhoneNumber)
            .HasColumnType("text")
            .HasColumnName("phone_number")
            .IsRequired(false);
        
        builder
            .HasIndex(u => u.PhoneNumber, "uq_user_phone_number")
            .IsUnique();

        builder
            .Property(u => u.PhoneNumberConfirmed)
            .HasColumnType("boolean")
            .HasColumnName("phone_number_confirmed")
            .HasDefaultValue(false);

        builder
            .Property(u => u.ConcurrencyStamp)
            .HasColumnType("text")
            .HasColumnName("concurrency_stamp")
            .IsConcurrencyToken()
            .IsRequired();

        builder
            .Property(u => u.SecurityStamp)
            .HasColumnType("text")
            .HasColumnName("security_stamp")
            .IsRequired();

        builder
            .Property(u => u.LockoutEnd)
            .HasColumnType("timestamptz")
            .HasColumnName("lockout_end");

        builder
            .Property(u => u.LockoutEnabled)
            .HasColumnType("boolean")
            .HasColumnName("lockout_enabled")
            .IsRequired();

        builder
            .Property(u => u.AccessFailedCount)
            .HasColumnType("integer")
            .HasColumnName("access_failed_count")
            .IsRequired();

        builder
            .Property(u => u.TwoFactorEnabled)
            .HasColumnType("boolean")
            .HasColumnName("two_factor_enabled")
            .IsRequired();
    }
}