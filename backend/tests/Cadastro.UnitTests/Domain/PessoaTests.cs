using Cadastro.Domain.Entities;
using Cadastro.Domain.Exceptions;

namespace Cadastro.UnitTests.Domain;

public class PessoaTests
{
    private static readonly DateOnly Nascimento = new(1990, 5, 20);

    [Fact]
    public void Construtor_DeveNormalizarDados()
    {
        var pessoa = new Pessoa("  Maria Silva  ", "529.982.247-25", Nascimento, " Maria@Email.COM ", "(11) 98765-4321");

        Assert.NotEqual(Guid.Empty, pessoa.Id);
        Assert.Equal("Maria Silva", pessoa.Nome);
        Assert.Equal("52998224725", pessoa.Cpf);
        Assert.Equal("maria@email.com", pessoa.Email);
        Assert.Equal("11987654321", pessoa.Telefone);
        Assert.Null(pessoa.DataAtualizacao);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeVazio(string nome)
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Pessoa(nome, "52998224725", Nascimento, "a@b.com", null));

        Assert.Equal("Nome é obrigatório.", ex.Message);
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoCpfInvalido()
    {
        Assert.Throws<DomainException>(() =>
            new Pessoa("Maria", "12345678900", Nascimento, "a@b.com", null));
    }

    [Fact]
    public void Atualizar_DeveAlterarDadosERegistrarDataAtualizacao()
    {
        var pessoa = new Pessoa("Maria", "52998224725", Nascimento, "a@b.com", null);

        pessoa.Atualizar("Maria Souza", new DateOnly(1991, 1, 1), "nova@b.com", "  ");

        Assert.Equal("Maria Souza", pessoa.Nome);
        Assert.Equal(new DateOnly(1991, 1, 1), pessoa.DataNascimento);
        Assert.Equal("nova@b.com", pessoa.Email);
        Assert.Null(pessoa.Telefone);
        Assert.NotNull(pessoa.DataAtualizacao);
    }
}
