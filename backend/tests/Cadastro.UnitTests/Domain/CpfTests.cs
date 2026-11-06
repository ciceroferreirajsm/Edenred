using Cadastro.Domain.Validation;

namespace Cadastro.UnitTests.Domain;

public class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    [InlineData(" 111.444.777-35 ")]
    public void EhValido_DeveRetornarTrue_ParaCpfsValidos(string cpf)
    {
        Assert.True(Cpf.EhValido(cpf));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("529.982.247-26")]   // dígito verificador errado
    [InlineData("52998224715")]      // primeiro dígito verificador errado
    [InlineData("000.000.000-00")]   // sequência repetida
    [InlineData("11111111111")]
    [InlineData("529982247250")]     // 12 dígitos
    public void EhValido_DeveRetornarFalse_ParaCpfsInvalidos(string? cpf)
    {
        Assert.False(Cpf.EhValido(cpf));
    }

    [Fact]
    public void Normalizar_DeveManterApenasDigitos()
    {
        Assert.Equal("52998224725", Cpf.Normalizar("529.982.247-25"));
    }

    [Fact]
    public void Formatar_DeveAplicarMascara()
    {
        Assert.Equal("529.982.247-25", Cpf.Formatar("52998224725"));
    }
}
