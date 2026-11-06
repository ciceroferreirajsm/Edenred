using Cadastro.Application.Abstractions;
using Cadastro.Application.Common;
using Cadastro.Domain.Entities;
using Cadastro.Domain.Validation;
using FluentValidation;

namespace Cadastro.Application.Veiculos;

public sealed class VeiculoService(
    IVeiculoRepository veiculos,
    IPessoaRepository pessoas,
    IUnitOfWork unitOfWork,
    IValidator<VeiculoRequest> validator,
    IValidator<VincularVeiculoRequest> vincularValidator,
    TimeProvider timeProvider) : IVeiculoService
{
    private int AnoAtual => timeProvider.GetLocalNow().Year;

    public async Task<VeiculoResponse> CriarAsync(VeiculoRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var pessoa = await ObterPessoaOuFalharAsync(request.PessoaId, cancellationToken);

        var placa = Placa.Normalizar(request.Placa);
        await GarantirPlacaUnicaAsync(placa, null, cancellationToken);

        var veiculo = new Veiculo(placa, request.AnoFabricacao, request.Marca, request.Modelo, pessoa.Id, AnoAtual);

        veiculos.Adicionar(veiculo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return VeiculoResponse.De(veiculo, pessoa.Nome);
    }

    public async Task<VeiculoResponse> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var veiculo = await ObterOuFalharAsync(id, cancellationToken);
        return VeiculoResponse.De(veiculo);
    }

    public async Task<PagedResult<VeiculoResponse>> ListarAsync(string? busca, Guid? pessoaId, Paginacao paginacao, CancellationToken cancellationToken = default)
    {
        var p = paginacao.Normalizar();
        var (itens, total) = await veiculos.ListarAsync(busca, pessoaId, p.Pagina, p.TamanhoPagina, cancellationToken);

        return new PagedResult<VeiculoResponse>(
            itens.Select(x => VeiculoResponse.De(x)).ToList(), p.Pagina, p.TamanhoPagina, total);
    }

    public async Task<VeiculoResponse> AtualizarAsync(Guid id, VeiculoRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var veiculo = await ObterOuFalharAsync(id, cancellationToken);

        var placa = Placa.Normalizar(request.Placa);
        if (placa != veiculo.Placa)
        {
            await GarantirPlacaUnicaAsync(placa, id, cancellationToken);
            veiculo.AlterarPlaca(placa);
        }

        var pessoaNome = veiculo.Pessoa?.Nome;
        if (request.PessoaId != veiculo.PessoaId)
        {
            var pessoa = await ObterPessoaOuFalharAsync(request.PessoaId, cancellationToken);
            veiculo.VincularPessoa(pessoa.Id);
            pessoaNome = pessoa.Nome;
        }

        veiculo.Atualizar(request.AnoFabricacao, request.Marca, request.Modelo, AnoAtual);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return VeiculoResponse.De(veiculo, pessoaNome);
    }

    public async Task<VeiculoResponse> VincularPessoaAsync(Guid id, VincularVeiculoRequest request, CancellationToken cancellationToken = default)
    {
        await vincularValidator.ValidateAndThrowAsync(request, cancellationToken);

        var veiculo = await ObterOuFalharAsync(id, cancellationToken);
        var pessoa = await ObterPessoaOuFalharAsync(request.PessoaId, cancellationToken);

        veiculo.VincularPessoa(pessoa.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return VeiculoResponse.De(veiculo, pessoa.Nome);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var veiculo = await ObterOuFalharAsync(id, cancellationToken);

        veiculos.Remover(veiculo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Veiculo> ObterOuFalharAsync(Guid id, CancellationToken cancellationToken) =>
        await veiculos.ObterPorIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Veículo '{id}' não encontrado.");

    private async Task<Pessoa> ObterPessoaOuFalharAsync(Guid pessoaId, CancellationToken cancellationToken) =>
        await pessoas.ObterPorIdAsync(pessoaId, cancellationToken)
        ?? throw new NotFoundException($"Pessoa '{pessoaId}' não encontrada.");

    private async Task GarantirPlacaUnicaAsync(string placa, Guid? ignorarId, CancellationToken cancellationToken)
    {
        if (await veiculos.PlacaJaCadastradaAsync(placa, ignorarId, cancellationToken))
            throw new ConflictException($"Já existe um veículo cadastrado com a placa {placa}.");
    }
}
