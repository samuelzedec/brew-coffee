using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Openiddict;

internal sealed class OpenIddictEntityFrameworkCoreScopeMap
    : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreScope<Guid>>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreScope<Guid>> builder)
    {
        builder.ToTable("openiddict_scopes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ConcurrencyToken)
            .HasColumnName("concurrency_token")
            .HasMaxLength(50)
            .IsConcurrencyToken();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(x => x.Descriptions)
            .HasColumnName("descriptions")
            .HasColumnType("text");

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200);

        builder.Property(x => x.DisplayNames)
            .HasColumnName("display_names")
            .HasColumnType("text");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200);

        builder.Property(x => x.Properties)
            .HasColumnName("properties")
            .HasColumnType("text");

        builder.Property(x => x.Resources)
            .HasColumnName("resources")
            .HasColumnType("text");

        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("ix_openiddict_scopes_name");
    }
}