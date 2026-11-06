using Cadastro.Application.Abstractions;
using Cadastro.Application.Common;
using Cadastro.Application.Pessoas;
using Cadastro.Domain.Entities;
using Cadastro.UnitTests.Support;
using FluentValidation;
using NSubstitute;

namespace Cadastro.UnitTests.Services;

public class PessoaServiceTests
{
    private readonly IPessoaRepository _repo = Substitute.For<IPessoaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly PessoaService _service;

    public PessoaServiceTests()
    {
        _service = new PessoaService(_repo, _uow, new PessoaRequestValidator(FixedTimeProvider.Em2025));
    }

    private static PessoaRequest Request(string cpf = "529.982.247-25") =>
        new("Maria Silva", cpf, new DateOnly(1990, 5, 20), "maria@email.com", "11987654321");

    private static Pessoa PessoaExistente(string cpf = "52998224725") =>
        new("Maria Silva", cpf, new DateOnly(1990, 5, 20), "maria@email.com", null);

    [Fact]
    public async Task CriarAsync_DevePersistirPessoa_QuandoDadosValidos()
    {
        _repo.CpfJaCadastradoAsync("52998224725", null, Arg.Any<CancellationToken>()).Returns(false);

        var response = await _service.CriarAsync(Request());

        Assert.Equal("Maria Silva", response.Nome);
        Assert.Equal("52998224725", response.Cpf);
        Assert.Equal(0, response.QuantidadeVeiculos);
        _repo.Received(1).Adicionar(Arg.Is<Pessoa>(p => p.Cpf == "52998224725"));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_DeveLancarConflict_QuandoCpfJaCadastrado()
    {
        _repo.CpfJaCadastradoAsync("52998224725", null, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.CriarAsync(Request()));

        Assert.Contains("529.982.247-25", ex.Message);
        _repo.DidNotReceive().Adicionar(Arg.Any<Pessoa>());
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_DeveLancarValidationException_QuandoDadosInvalidos()
    {
        var request = Request() with { Nome = "", Email = "invalido" };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CriarAsync(request));

        Assert.Contains(ex.Errors, e => e.PropertyName == "Nome");
        Assert.Contains(ex.Errors, e => e.PropertyName == "Email");
        await _repo.DidNotReceive().CpfJaCadastradoAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ObterAsync_DeveLancarNotFound_QuandoPessoaNaoExiste()
    {
        _repo.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Pessoa?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AtualizarAsync_NaoDeveVerificarDuplicidade_QuandoCpfNaoMudou()
    {
        var pessoa = PessoaExistente();
        _repo.ObterPorIdAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(pessoa);

        var response = await _service.AtualizarAsync(pessoa.Id, Request() with { Nome = "Maria Souza" });

        Assert.Equal("Maria Souza", response.Nome);
        await _repo.DidNotReceive().CpfJaCadastradoAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarConflict_QuandoNovoCpfPertenceAOutraPessoa()
    {
        var pessoa = PessoaExistente();
        _repo.ObterPorIdAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(pessoa);
        _repo.CpfJaCadastradoAsync("11144477735", pessoa.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.AtualizarAsync(pessoa.Id, Request("111.444.777-35")));

        Assert.Equal("52998224725", pessoa.Cpf);
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_DeveAlterarCpf_QuandoNovoCpfDisponivel()
    {
        var pessoa = PessoaExistente();
        _repo.ObterPorIdAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(pessoa);
        _repo.CpfJaCadastradoAsync("11144477735", pessoa.Id, Arg.Any<CancellationToken>()).Returns(false);

        var response = await _service.AtualizarAsync(pessoa.Id, Request("111.444.777-35"));

        Assert.Equal("11144477735", response.Cpf);
    }

    [Fact]
    public async Task ExcluirAsync_DeveLancarConflict_QuandoPessoaPossuiVeiculos()
    {
        var pessoa = PessoaExistente();
        _repo.ObterPorIdAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(pessoa);
        _repo.ContarVeiculosAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(2);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.ExcluirAsync(pessoa.Id));

        Assert.Contains("2 veículo(s)", ex.Message);
        _repo.DidNotReceive().Remover(Arg.Any<Pessoa>());
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverPessoa_QuandoSemVeiculos()
    {
        var pessoa = PessoaExistente();
        _repo.ObterPorIdAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(pessoa);
        _repo.ContarVeiculosAsync(pessoa.Id, Arg.Any<CancellationToken>()).Returns(0);

        await _service.ExcluirAsync(pessoa.Id);

        _repo.Received(1).Remover(pessoa);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListarAsync_DeveNormalizarPaginacaoERetornarMetadados()
    {
        _repo.ListarAsync(null, 1, 100, Arg.Any<CancellationToken>())
            .Returns(([PessoaExistente()], 101));

        var result = await _service.ListarAsync(null, new Paginacao(0, 500));

        Assert.Equal(1, result.Pagina);
        Assert.Equal(100, result.TamanhoPagina);
        Assert.Equal(101, result.TotalItens);
        Assert.Equal(2, result.TotalPaginas);
        Assert.Single(result.Itens);
    }
}
