using Cadastro.Application.Abstractions;
using Cadastro.Application.Common;
using Cadastro.Application.Veiculos;
using Cadastro.Domain.Entities;
using Cadastro.UnitTests.Support;
using FluentValidation;
using NSubstitute;

namespace Cadastro.UnitTests.Services;

public class VeiculoServiceTests
{
    private readonly IVeiculoRepository _veiculos = Substitute.For<IVeiculoRepository>();
    private readonly IPessoaRepository _pessoas = Substitute.For<IPessoaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly VeiculoService _service;

    private readonly Pessoa _dono = new("João Souza", "52998224725", new DateOnly(1985, 1, 10), "joao@email.com", null);
    private readonly Pessoa _outraPessoa = new("Ana Lima", "11144477735", new DateOnly(1992, 3, 3), "ana@email.com", null);

    public VeiculoServiceTests()
    {
        var tempo = FixedTimeProvider.Em2025;
        _service = new VeiculoService(
            _veiculos, _pessoas, _uow,
            new VeiculoRequestValidator(tempo),
            new VincularVeiculoRequestValidator(),
            tempo);

        _pessoas.ObterPorIdAsync(_dono.Id, Arg.Any<CancellationToken>()).Returns(_dono);
        _pessoas.ObterPorIdAsync(_outraPessoa.Id, Arg.Any<CancellationToken>()).Returns(_outraPessoa);
    }

    private VeiculoRequest Request(string placa = "abc-1d23", int ano = 2020) =>
        new(placa, ano, "Fiat", "Argo", _dono.Id);

    private Veiculo VeiculoExistente(string placa = "ABC1D23") =>
        new(placa, 2020, "Fiat", "Argo", _dono.Id, 2025);

    [Fact]
    public async Task CriarAsync_DevePersistirVeiculoComPlacaNormalizada()
    {
        _veiculos.PlacaJaCadastradaAsync("ABC1D23", null, Arg.Any<CancellationToken>()).Returns(false);

        var response = await _service.CriarAsync(Request());

        Assert.Equal("ABC1D23", response.Placa);
        Assert.Equal(_dono.Id, response.PessoaId);
        Assert.Equal("João Souza", response.PessoaNome);
        _veiculos.Received(1).Adicionar(Arg.Is<Veiculo>(v => v.Placa == "ABC1D23"));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CriarAsync_DeveLancarConflict_QuandoPlacaJaCadastrada()
    {
        _veiculos.PlacaJaCadastradaAsync("ABC1D23", null, Arg.Any<CancellationToken>()).Returns(true);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _service.CriarAsync(Request()));

        Assert.Contains("ABC1D23", ex.Message);
        _veiculos.DidNotReceive().Adicionar(Arg.Any<Veiculo>());
    }

    [Fact]
    public async Task CriarAsync_DeveLancarNotFound_QuandoPessoaNaoExiste()
    {
        var request = Request() with { PessoaId = Guid.NewGuid() };

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CriarAsync(request));
        _veiculos.DidNotReceive().Adicionar(Arg.Any<Veiculo>());
    }

    [Fact]
    public async Task CriarAsync_DeveLancarValidationException_QuandoAnoMaiorQueAnoAtual()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CriarAsync(Request(ano: 2026)));

        Assert.Contains(ex.Errors, e => e.PropertyName == nameof(VeiculoRequest.AnoFabricacao));
        await _pessoas.DidNotReceive().ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarConflict_QuandoNovaPlacaPertenceAOutroVeiculo()
    {
        var veiculo = VeiculoExistente();
        _veiculos.ObterPorIdAsync(veiculo.Id, Arg.Any<CancellationToken>()).Returns(veiculo);
        _veiculos.PlacaJaCadastradaAsync("XYZ9876", veiculo.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.AtualizarAsync(veiculo.Id, Request("XYZ-9876")));

        Assert.Equal("ABC1D23", veiculo.Placa);
        await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarDadosETransferirProprietario()
    {
        var veiculo = VeiculoExistente();
        _veiculos.ObterPorIdAsync(veiculo.Id, Arg.Any<CancellationToken>()).Returns(veiculo);

        var request = new VeiculoRequest("ABC1D23", 2021, "Fiat", "Pulse", _outraPessoa.Id);
        var response = await _service.AtualizarAsync(veiculo.Id, request);

        Assert.Equal(2021, response.AnoFabricacao);
        Assert.Equal("Pulse", response.Modelo);
        Assert.Equal(_outraPessoa.Id, response.PessoaId);
        Assert.Equal("Ana Lima", response.PessoaNome);
        await _veiculos.DidNotReceive().PlacaJaCadastradaAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VincularPessoaAsync_DeveTransferirVeiculoParaOutraPessoa()
    {
        var veiculo = VeiculoExistente();
        _veiculos.ObterPorIdAsync(veiculo.Id, Arg.Any<CancellationToken>()).Returns(veiculo);

        var response = await _service.VincularPessoaAsync(veiculo.Id, new VincularVeiculoRequest(_outraPessoa.Id));

        Assert.Equal(_outraPessoa.Id, response.PessoaId);
        Assert.Equal(_outraPessoa.Id, veiculo.PessoaId);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VincularPessoaAsync_DeveLancarNotFound_QuandoPessoaNaoExiste()
    {
        var veiculo = VeiculoExistente();
        _veiculos.ObterPorIdAsync(veiculo.Id, Arg.Any<CancellationToken>()).Returns(veiculo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.VincularPessoaAsync(veiculo.Id, new VincularVeiculoRequest(Guid.NewGuid())));

        Assert.Equal(_dono.Id, veiculo.PessoaId);
    }

    [Fact]
    public async Task VincularPessoaAsync_DeveLancarValidationException_QuandoPessoaIdVazio()
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.VincularPessoaAsync(Guid.NewGuid(), new VincularVeiculoRequest(Guid.Empty)));
    }

    [Fact]
    public async Task ExcluirAsync_DeveLancarNotFound_QuandoVeiculoNaoExiste()
    {
        _veiculos.ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Veiculo?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ExcluirAsync(Guid.NewGuid()));
        _veiculos.DidNotReceive().Remover(Arg.Any<Veiculo>());
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverVeiculo()
    {
        var veiculo = VeiculoExistente();
        _veiculos.ObterPorIdAsync(veiculo.Id, Arg.Any<CancellationToken>()).Returns(veiculo);

        await _service.ExcluirAsync(veiculo.Id);

        _veiculos.Received(1).Remover(veiculo);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
