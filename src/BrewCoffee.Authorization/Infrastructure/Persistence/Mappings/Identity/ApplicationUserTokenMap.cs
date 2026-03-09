using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Identity;

internal sealed class ApplicationUserTokenMap : IEntityTypeConfiguration<ApplicationUserToken>
{
    public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
    {
        builder.ToTable("user_token");

        builder
            .HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name })
            .HasName("pk_user_token_user_id_login_provider_name");

        builder
            .Property(ut => ut.UserId)
            .HasColumnType("uuid")
            .HasColumnName("user_id")
            .IsRequired();

        builder
            .Property(ut => ut.LoginProvider)
            .HasColumnType("text")
            .HasColumnName("login_provider")
            .IsRequired();

        builder
            .Property(ut => ut.Name)
            .HasColumnType("text")
            .HasColumnName("name")
            .IsRequired();

        builder
            .Property(ut => ut.Value)
            .HasColumnType("text")
            .HasColumnName("value")
            .IsRequired();
    }
}