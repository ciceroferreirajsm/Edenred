using Cadastro.Domain.Entities;
using Cadastro.Domain.Exceptions;

namespace Cadastro.UnitTests.Domain;

public class VeiculoTests
{
    private const int AnoAtual = 2025;
    private static readonly Guid PessoaId = Guid.NewGuid();

    [Fact]
    public void Construtor_DeveNormalizarPlacaEVincularPessoa()
    {
        var veiculo = new Veiculo("abc-1d23", 2020, " Fiat ", " Argo ", PessoaId, AnoAtual);

        Assert.Equal("ABC1D23", veiculo.Placa);
        Assert.Equal("Fiat", veiculo.Marca);
        Assert.Equal("Argo", veiculo.Modelo);
        Assert.Equal(PessoaId, veiculo.PessoaId);
        Assert.Null(veiculo.DataAtualizacao);
    }

    [Fact]
    public void Construtor_DeveAceitarAnoIgualAoAnoAtual()
    {
        var veiculo = new Veiculo("ABC1234", AnoAtual, "VW", "Gol", PessoaId, AnoAtual);
        Assert.Equal(AnoAtual, veiculo.AnoFabricacao);
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoAnoMaiorQueAnoAtual()
    {
        Assert.Throws<DomainException>(() =>
            new Veiculo("ABC1234", AnoAtual + 1, "VW", "Gol", PessoaId, AnoAtual));
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoSemPessoa()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Veiculo("ABC1234", 2020, "VW", "Gol", Guid.Empty, AnoAtual));

        Assert.Contains("vinculado a uma pessoa", ex.Message);
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoPlacaInvalida()
    {
        Assert.Throws<DomainException>(() =>
            new Veiculo("XX-99", 2020, "VW", "Gol", PessoaId, AnoAtual));
    }

    [Fact]
    public void VincularPessoa_DeveTransferirVeiculo()
    {
        var veiculo = new Veiculo("ABC1234", 2020, "VW", "Gol", PessoaId, AnoAtual);
        var novoDono = Guid.NewGuid();

        veiculo.VincularPessoa(novoDono);

        Assert.Equal(novoDono, veiculo.PessoaId);
        Assert.NotNull(veiculo.DataAtualizacao);
    }
}
