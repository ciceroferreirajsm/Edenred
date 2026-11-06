using Cadastro.Domain.Validation;

namespace Cadastro.UnitTests.Domain;

public class PlacaTests
{
    [Theory]
    [InlineData("ABC1234")]   // padrão antigo
    [InlineData("abc-1234")]  // minúsculas e hífen
    [InlineData("ABC1D23")]   // Mercosul
    [InlineData("bra 2e19")]
    public void EhValida_DeveAceitarPadraoAntigoEMercosul(string placa)
    {
        Assert.True(Placa.EhValida(placa));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("AB12345")]
    [InlineData("ABCD123")]
    [InlineData("ABC12345")]
    [InlineData("1BC1234")]
    [InlineData("ABC1DD3")]
    public void EhValida_DeveRejeitarFormatosInvalidos(string? placa)
    {
        Assert.False(Placa.EhValida(placa));
    }

    [Theory]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData(" bra 2e19 ", "BRA2E19")]
    public void Normalizar_DeveRemoverSeparadoresEConverterParaMaiusculas(string entrada, string esperado)
    {
        Assert.Equal(esperado, Placa.Normalizar(entrada));
    }
}
