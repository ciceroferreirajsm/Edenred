using Cadastro.Application.Pessoas;
using Cadastro.UnitTests.Support;

namespace Cadastro.UnitTests.Validators;

public class PessoaRequestValidatorTests
{
    private readonly PessoaRequestValidator _validator = new(FixedTimeProvider.Em2025);

    private static PessoaRequest RequestValido() =>
        new("Maria Silva", "529.982.247-25", new DateOnly(1990, 5, 20), "maria@email.com", "(11) 98765-4321");

    [Fact]
    public void DeveSerValido_QuandoTodosOsCamposCorretos()
    {
        var result = _validator.Validate(RequestValido());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void DeveSerValido_QuandoTelefoneNaoInformado()
    {
        var result = _validator.Validate(RequestValido() with { Telefone = null });
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DeveFalhar_QuandoNomeNaoInformado(string? nome)
    {
        var result = _validator.Validate(RequestValido() with { Nome = nome! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PessoaRequest.Nome) && e.ErrorMessage == "Nome é obrigatório.");
    }

    [Fact]
    public void DeveFalhar_QuandoNomeExcedeTamanhoMaximo()
    {
        var result = _validator.Validate(RequestValido() with { Nome = new string('a', 151) });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PessoaRequest.Nome));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345678900")]
    [InlineData("111.111.111-11")]
    public void DeveFalhar_QuandoCpfInvalido(string cpf)
    {
        var result = _validator.Validate(RequestValido() with { Cpf = cpf });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PessoaRequest.Cpf));
    }

    [Theory]
    [InlineData("")]
    [InlineData("maria")]
    [InlineData("maria@")]
    [InlineData("maria@email")]
    [InlineData("maria@email.c")]
    [InlineData("maria silva@email.com")]
    [InlineData("@email.com")]
    public void DeveFalhar_QuandoEmailInvalido(string email)
    {
        var result = _validator.Validate(RequestValido() with { Email = email });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PessoaRequest.Email));
    }

    [Theory]
    [InlineData("maria@email.com")]
    [InlineData("maria.silva+tag@empresa.com.br")]
    public void DeveAceitar_EmailsValidos(string email)
    {
        var result = _validator.Validate(RequestValido() with { Email = email });
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(PessoaRequest.Email));
    }

    [Fact]
    public void DeveFalhar_QuandoDataNascimentoNoFuturo()
    {
        var result = _validator.Validate(RequestValido() with { DataNascimento = new DateOnly(2025, 6, 16) });
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Data de nascimento não pode estar no futuro.");
    }

    [Fact]
    public void DeveFalhar_QuandoDataNascimentoNaoInformada()
    {
        var result = _validator.Validate(RequestValido() with { DataNascimento = default });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PessoaRequest.DataNascimento));
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("(11) 9876-54321-0")]
    [InlineData("abc11987654321")]
    public void DeveFalhar_QuandoTelefoneInvalido(string telefone)
    {
        var result = _validator.Validate(RequestValido() with { Telefone = telefone });
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PessoaRequest.Telefone));
    }
}
