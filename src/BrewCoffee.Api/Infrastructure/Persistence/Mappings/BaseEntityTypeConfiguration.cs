using BrewCoffee.Api.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrewCoffee.Api.Infrastructure.Persistence.Mappings;

public abstract class BaseEntityTypeConfiguration<T> : IEntityTypeConfiguration<T>
    where T : Entity
{
    /// <summary>
    /// Configura o mapeamento do tipo de entidade fornecido para o modelo de banco de dados.
    /// Este método define as propriedades, chaves e filtros padrão para a entidade base,
    /// permitindo extensões adicionais em classes derivadas.
    /// </summary>
    /// <param name="builder">
    /// O construtor de configuração do tipo de entidade, usado para definir o mapeamento do modelo.
    /// </param>
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(GetTableName());

        builder.HasKey(x => x.Id)
            .HasName($"pk_{GetTableName()}_id");

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamptz")
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamptz")
            .HasColumnName("modified_at");

        builder.Property(x => x.DeletedAt)
            .HasColumnType("timestamptz")
            .HasColumnName("deleted_at");

        ConfigureEntity(builder);
        ConfigureEntityQueryFilter(builder);
    }

    /// <summary>
    /// Obtém o nome da tabela no banco de dados para o tipo mapeado.
    /// </summary>
    /// <returns>
    /// O nome da tabela correspondente.
    /// </returns>
    protected abstract string GetTableName();

    /// <summary>
    /// Configura propriedades adicionais ou relações específicas para a entidade do tipo fornecido.
    /// Este método deve ser implementado em classes derivadas para personalizar o mapeamento.
    /// </summary>
    /// <param name="builder">
    /// O construtor de configuração do tipo de entidade.
    /// </param>
    protected abstract void ConfigureEntity(EntityTypeBuilder<T> builder);

    /// <summary>
    /// Configura o filtro de consulta padrão para a entidade, excluindo registros que foram logicamente excluídos.
    /// Este método pode ser sobrescrito para personalizar os critérios de filtro para a entidade específica.
    /// </summary>
    /// <param name="builder">
    /// O construtor de configuração do tipo de entidade.
    /// </param>
    protected virtual void ConfigureEntityQueryFilter(EntityTypeBuilder<T> builder)
        => builder.HasQueryFilter(x => !x.DeletedAt.HasValue);
}