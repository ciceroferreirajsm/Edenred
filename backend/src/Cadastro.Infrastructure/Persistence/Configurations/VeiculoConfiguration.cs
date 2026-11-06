using Cadastro.Application.Veiculos;
using Cadastro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cadastro.Infrastructure.Persistence.Configurations;

public sealed class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("Veiculos");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Placa).IsRequired().HasMaxLength(7).IsFixedLength();
        builder.Property(v => v.AnoFabricacao).IsRequired();
        builder.Property(v => v.Marca).IsRequired().HasMaxLength(VeiculoRequestValidator.MarcaTamanhoMaximo);
        builder.Property(v => v.Modelo).IsRequired().HasMaxLength(VeiculoRequestValidator.ModeloTamanhoMaximo);
        builder.Property(v => v.PessoaId).IsRequired();
        builder.Property(v => v.DataCadastro).IsRequired();

        builder.HasIndex(v => v.Placa).IsUnique().HasDatabaseName("UX_Veiculos_Placa");
        builder.HasIndex(v => v.PessoaId).HasDatabaseName("IX_Veiculos_PessoaId");
    }
}
