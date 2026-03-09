using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Openiddict;

internal sealed class OpenIddictEntityFrameworkCoreTokenMap
    : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreToken<Guid>>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreToken<Guid>> builder)
    {
        builder.ToTable("openiddict_tokens");

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

        builder.Property(x => x.ExpirationDate)
            .HasColumnName("expiration_date");

        builder.Property(x => x.Payload)
            .HasColumnName("payload")
            .HasColumnType("text");

        builder.Property(x => x.Properties)
            .HasColumnName("properties")
            .HasColumnType("text");

        builder.Property(x => x.RedemptionDate)
            .HasColumnName("redemption_date");

        builder.Property(x => x.ReferenceId)
            .HasColumnName("reference_id")
            .HasMaxLength(100);

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
            .WithMany(x => x.Tokens)
            .HasForeignKey("application_id")
            .HasConstraintName("fk_openiddict_tokens_application_id")
            .IsRequired(false);

        builder.HasOne(x => x.Authorization)
            .WithMany(x => x.Tokens)
            .HasForeignKey("authorization_id")
            .HasConstraintName("fk_openiddict_tokens_authorization_id")
            .IsRequired(false);

        builder.HasIndex("application_id")
            .HasDatabaseName("ix_openiddict_tokens_application_id");

        builder.HasIndex("authorization_id")
            .HasDatabaseName("ix_openiddict_tokens_authorization_id");

        builder.HasIndex(x => x.ReferenceId)
            .IsUnique()
            .HasDatabaseName("ix_openiddict_tokens_reference_id");

        builder.HasIndex(x => new { x.Status, x.Subject, x.Type })
            .HasDatabaseName("ix_openiddict_tokens_status_subject_type");
    }
}