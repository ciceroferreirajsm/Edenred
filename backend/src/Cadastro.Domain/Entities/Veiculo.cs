using Cadastro.Domain.Exceptions;

namespace Cadastro.Domain.Entities;

public class Veiculo
{
    /// <summary>Primeiro ano aceito para fabricação (sanidade de dados).</summary>
    public const int AnoMinimoFabricacao = 1886;

    // Construtor exigido pelo EF Core.
    private Veiculo() { }

    public Veiculo(string placa, int anoFabricacao, string marca, string modelo, Guid pessoaId, int anoAtual)
    {
        Id = Guid.NewGuid();
        DataCadastro = DateTime.UtcNow;
        Placa = NormalizarPlaca(placa);
        Atualizar(anoFabricacao, marca, modelo, anoAtual);
        VincularPessoa(pessoaId);
        DataAtualizacao = null;
    }

    public Guid Id { get; private set; }
    public string Placa { get; private set; } = string.Empty;
    public int AnoFabricacao { get; private set; }
    public string Marca { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public Guid PessoaId { get; private set; }
    public Pessoa? Pessoa { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    public void Atualizar(int anoFabricacao, string marca, string modelo, int anoAtual)
    {
        if (anoFabricacao < AnoMinimoFabricacao || anoFabricacao > anoAtual)
            throw new DomainException($"Ano de fabricação deve estar entre {AnoMinimoFabricacao} e {anoAtual}.");

        if (string.IsNullOrWhiteSpace(marca))
            throw new DomainException("Marca é obrigatória.");

        if (string.IsNullOrWhiteSpace(modelo))
            throw new DomainException("Modelo é obrigatório.");

        AnoFabricacao = anoFabricacao;
        Marca = marca.Trim();
        Modelo = modelo.Trim();
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AlterarPlaca(string placa)
    {
        Placa = NormalizarPlaca(placa);
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>Vincula (ou transfere) o veículo para uma pessoa.</summary>
    public void VincularPessoa(Guid pessoaId)
    {
        if (pessoaId == Guid.Empty)
            throw new DomainException("O veículo deve estar vinculado a uma pessoa.");

        PessoaId = pessoaId;
        DataAtualizacao = DateTime.UtcNow;
    }

    private static string NormalizarPlaca(string placa)
    {
        if (!Validation.Placa.EhValida(placa))
            throw new DomainException("Placa inválida. Use o formato ABC1234 ou ABC1D23 (Mercosul).");

        return Validation.Placa.Normalizar(placa);
    }
}
