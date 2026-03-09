using BrewCoffee.Authorization.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Authorization.Infrastructure.Persistence.Mappings.Identity;

internal sealed class ApplicationUserClaimMap : IEntityTypeConfiguration<ApplicationUserClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
    {
        builder.ToTable("user_claim");
        
        builder
            .HasKey(u => u.Id)
            .HasName("pk_user_claim_id");

        builder
            .Property(rc => rc.Id)
            .HasColumnType("integer")
            .HasColumnName("id")
            .UseIdentityColumn();

        builder
            .Property(uc => uc.UserId)
            .HasColumnType("uuid")
            .HasColumnName("user_id")
            .IsRequired();

        builder
            .Property(uc => uc.ClaimType)
            .HasColumnType("text")
            .HasColumnName("claim_type")
            .IsRequired();
       
        builder
            .Property(uc => uc.ClaimValue)
            .HasColumnType("text")
            .HasColumnName("claim_value")
            .IsRequired();

        builder
            .HasIndex(uc => uc.UserId)
            .HasDatabaseName("ix_user_claim_user_id");
    }
}