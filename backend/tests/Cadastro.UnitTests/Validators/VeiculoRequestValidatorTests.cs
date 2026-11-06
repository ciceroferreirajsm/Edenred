using Cadastro.Application.Veiculos;
using Cadastro.UnitTests.Support;

namespace Cadastro.UnitTests.Validators;

public class VeiculoRequestValidatorTests
{
    private readonly VeiculoRequestValidator _validator = new(FixedTimeProvider.Em2025);

    private static VeiculoRequest RequestValido() =>
        new("ABC1D23", 2020, "Fiat", "Argo", Guid.NewGuid());

    [Fact]
    public void DeveSerValido_QuandoTodosOsCamposCorretos()
    {
        Assert.True(_validator.Validate(RequestValido()).IsValid);
    }

    [Fact]
    public void DeveAceitar_AnoIgualAoAnoAtual()
    {
        Assert.True(_validator.Validate(RequestValido() with { AnoFabricacao = 2025 }).IsValid);
    }

    [Fact]
    public void DeveFalhar_QuandoAnoMaiorQueAnoAtual()
    {
        var result = _validator.Validate(RequestValido() with { AnoFabricacao = 2026 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(VeiculoRequest.AnoFabricacao) &&
            e.ErrorMessage == "Ano de fabricação não pode ser maior que o ano atual (2025).");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1885)]
    public void DeveFalhar_QuandoAnoMuitoAntigo(int ano)
    {
        var result = _validator.Validate(RequestValido() with { AnoFabricacao = ano });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VeiculoRequest.AnoFabricacao));
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB-123")]
    [InlineData("ABCD1234")]
    public void DeveFalhar_QuandoPlacaInvalida(string placa)
    {
        var result = _validator.Validate(RequestValido() with { Placa = placa });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VeiculoRequest.Placa));
    }

    [Fact]
    public void DeveFalhar_QuandoMarcaEModeloVazios()
    {
        var result = _validator.Validate(RequestValido() with { Marca = "", Modelo = " " });

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Marca é obrigatória.");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Modelo é obrigatório.");
    }

    [Fact]
    public void DeveFalhar_QuandoPessoaNaoInformada()
    {
        var result = _validator.Validate(RequestValido() with { PessoaId = Guid.Empty });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VeiculoRequest.PessoaId));
    }
}
