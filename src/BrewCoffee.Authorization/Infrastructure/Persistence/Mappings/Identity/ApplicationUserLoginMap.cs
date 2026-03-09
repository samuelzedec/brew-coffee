using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Identity;

internal sealed class ApplicationUserLoginMap : IEntityTypeConfiguration<ApplicationUserLogin>
{
    public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
    {
        builder.ToTable("user_login");
        
        builder
            .HasKey(ul => new { ul.UserId, ul.LoginProvider, ul.ProviderKey })
            .HasName("pk_user_login_user_id_login_provider_provider_key");
        
        builder
            .Property(ul => ul.UserId)
            .HasColumnType("uuid")
            .HasColumnName("user_id")
            .IsRequired();
        
        builder
            .Property(ul => ul.LoginProvider)
            .HasColumnType("text")
            .HasColumnName("login_provider")
            .IsRequired();
        
        builder
            .Property(ul => ul.ProviderKey)
            .HasColumnType("text")
            .HasColumnName("provider_key")
            .IsRequired();

        builder
            .Property(ul => ul.ProviderDisplayName)
            .HasColumnType("text")
            .HasColumnName("provider_display_name")
            .IsRequired(false);
    }
}