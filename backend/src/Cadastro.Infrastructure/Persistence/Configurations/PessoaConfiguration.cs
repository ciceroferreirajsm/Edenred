using Cadastro.Application.Pessoas;
using Cadastro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cadastro.Infrastructure.Persistence.Configurations;

public sealed class PessoaConfiguration : IEntityTypeConfiguration<Pessoa>
{
    public void Configure(EntityTypeBuilder<Pessoa> builder)
    {
        builder.ToTable("Pessoas");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(PessoaRequestValidator.NomeTamanhoMaximo);
        builder.Property(p => p.Cpf).IsRequired().HasMaxLength(11).IsFixedLength();
        builder.Property(p => p.DataNascimento).IsRequired();
        builder.Property(p => p.Email).IsRequired().HasMaxLength(PessoaRequestValidator.EmailTamanhoMaximo);
        builder.Property(p => p.Telefone).HasMaxLength(11);
        builder.Property(p => p.DataCadastro).IsRequired();

        // Garantia de unicidade também no banco (protege contra condições de corrida).
        builder.HasIndex(p => p.Cpf).IsUnique().HasDatabaseName("UX_Pessoas_Cpf");
        builder.HasIndex(p => p.Nome).HasDatabaseName("IX_Pessoas_Nome");

        builder.HasMany(p => p.Veiculos)
            .WithOne(v => v.Pessoa)
            .HasForeignKey(v => v.PessoaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(p => p.Veiculos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
