using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Identity;

internal sealed class IdentityUserPasskeyMap : IEntityTypeConfiguration<IdentityUserPasskey<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserPasskey<Guid>> builder)
    {
        builder.ToTable("user_passkey");

        builder.HasKey(p => p.CredentialId)
            .HasName("pk_user_passkey_credential_id");

        builder
            .Property(p => p.CredentialId)
            .HasColumnType("bytea")
            .HasColumnName("credential_id")
            .HasMaxLength(1024)
            .IsRequired();

        builder
            .Property(p => p.UserId)
            .HasColumnType("uuid")
            .HasColumnName("user_id")
            .IsRequired();

        builder
            .HasIndex(p => p.UserId)
            .HasDatabaseName("ix_user_passkey_user_id");

        builder
            .OwnsOne(p => p.Data, data =>
            {
                data.ToJson("data");

                data.Property(d => d.Name)
                    .HasJsonPropertyName("name");

                data.Property(d => d.PublicKey)
                    .HasJsonPropertyName("public_key")
                    .IsRequired();

                data.Property(d => d.CreatedAt)
                    .HasJsonPropertyName("created_at")
                    .IsRequired();

                data.Property(d => d.SignCount)
                    .HasJsonPropertyName("sign_count")
                    .IsRequired();

                data.Property(d => d.Transports)
                    .HasJsonPropertyName("transports");

                data.Property(d => d.IsUserVerified)
                    .HasJsonPropertyName("is_user_verified")
                    .IsRequired();

                data.Property(d => d.IsBackupEligible)
                    .HasJsonPropertyName("is_backup_eligible")
                    .IsRequired();

                data.Property(d => d.IsBackedUp)
                    .HasJsonPropertyName("is_backed_up")
                    .IsRequired();

                data.Property(d => d.AttestationObject)
                    .HasJsonPropertyName("attestation_object")
                    .IsRequired();

                data.Property(d => d.ClientDataJson)
                    .HasJsonPropertyName("client_data_json")
                    .IsRequired();
            });
    }
}