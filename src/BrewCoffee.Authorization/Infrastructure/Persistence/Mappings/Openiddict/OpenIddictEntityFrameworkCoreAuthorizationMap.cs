using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Openiddict;

internal sealed class OpenIddictEntityFrameworkCoreAuthorizationMap
    : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreAuthorization<Guid>>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreAuthorization<Guid>> builder)
    {
        builder.ToTable("openiddict_authorizations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ConcurrencyToken)
            .HasColumnName("concurrency_token")
            .HasMaxLength(50)
            .IsConcurrencyToken();

        builder.Property(x => x.CreationDate)
            .HasColumnName("creation_date");

        builder.Property(x => x.Properties)
            .HasColumnName("properties")
            .HasColumnType("text");

        builder.Property(x => x.Scopes)
            .HasColumnName("scopes")
            .HasColumnType("text");

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(50);

        builder.Property(x => x.Subject)
            .HasColumnName("subject")
            .HasMaxLength(400);

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(50);

        builder.HasOne(x => x.Application)
            .WithMany()
            .HasForeignKey("application_id")
            .HasConstraintName("fk_openiddict_authorizations_application_id")
            .IsRequired(false);

        builder.HasIndex("application_id")
            .HasDatabaseName("ix_openiddict_authorizations_application_id");

        builder.HasIndex(x => new { x.Status, x.Subject, x.Type })
            .HasDatabaseName("ix_openiddict_authorizations_status_subject_type");
    }
}