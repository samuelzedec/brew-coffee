using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Openiddict;

internal sealed class OpenIddictEntityFrameworkCoreApplicationMap
    : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreApplication<Guid>>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreApplication<Guid>> builder)
    {
        builder.ToTable("openiddict_applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ApplicationType)
            .HasColumnName("application_type")
            .HasColumnType("text");

        builder.Property(x => x.ClientId)
            .HasColumnName("client_id")
            .HasMaxLength(100);

        builder.Property(x => x.ClientSecret)
            .HasColumnName("client_secret")
            .HasColumnType("text");

        builder.Property(x => x.ClientType)
            .HasColumnName("client_type")
            .HasMaxLength(50);

        builder.Property(x => x.ConcurrencyToken)
            .HasColumnName("concurrency_token")
            .HasMaxLength(50)
            .IsConcurrencyToken();

        builder.Property(x => x.ConsentType)
            .HasColumnName("consent_type")
            .HasMaxLength(50);

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200);

        builder.Property(x => x.DisplayNames)
            .HasColumnName("display_names")
            .HasColumnType("text");

        builder.Property(x => x.JsonWebKeySet)
            .HasColumnName("json_web_key_set")
            .HasColumnType("text");

        builder.Property(x => x.Permissions)
            .HasColumnName("permissions")
            .HasColumnType("text");

        builder.Property(x => x.PostLogoutRedirectUris)
            .HasColumnName("post_logout_redirect_uris")
            .HasColumnType("text");

        builder.Property(x => x.Properties)
            .HasColumnName("properties")
            .HasColumnType("text");

        builder.Property(x => x.RedirectUris)
            .HasColumnName("redirect_uris")
            .HasColumnType("text");

        builder.Property(x => x.Requirements)
            .HasColumnName("requirements")
            .HasColumnType("text");

        builder.Property(x => x.Settings)
            .HasColumnName("settings")
            .HasColumnType("text");

        builder.HasMany(x => x.Authorizations)
            .WithOne(x => x.Application)
            .HasForeignKey("application_id")
            .HasConstraintName("fk_openiddict_authorizations_application_id")
            .IsRequired(false);

        builder.HasMany(x => x.Tokens)
            .WithOne(x => x.Application)
            .HasForeignKey("application_id")
            .HasConstraintName("fk_openiddict_tokens_application_id")
            .IsRequired(false);

        builder.HasIndex(x => x.ClientId)
            .IsUnique()
            .HasDatabaseName("ix_openiddict_applications_client_id");
    }
}