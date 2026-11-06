using Cadastro.Application.Abstractions;
using Cadastro.Application.Common;
using Cadastro.Domain.Entities;
using Cadastro.Domain.Validation;
using FluentValidation;

namespace Cadastro.Application.Pessoas;

public sealed class PessoaService(
    IPessoaRepository pessoas,
    IUnitOfWork unitOfWork,
    IValidator<PessoaRequest> validator) : IPessoaService
{
    public async Task<PessoaResponse> CriarAsync(PessoaRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var cpf = Cpf.Normalizar(request.Cpf);
        await GarantirCpfUnicoAsync(cpf, null, cancellationToken);

        var pessoa = new Pessoa(request.Nome, cpf, request.DataNascimento, request.Email, request.Telefone);

        pessoas.Adicionar(pessoa);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return PessoaResponse.De(pessoa, 0);
    }

    public async Task<PessoaResponse> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pessoa = await ObterOuFalharAsync(id, cancellationToken);
        return PessoaResponse.De(pessoa);
    }

    public async Task<PagedResult<PessoaResponse>> ListarAsync(string? busca, Paginacao paginacao, CancellationToken cancellationToken = default)
    {
        var p = paginacao.Normalizar();
        var (itens, total) = await pessoas.ListarAsync(busca, p.Pagina, p.TamanhoPagina, cancellationToken);

        return new PagedResult<PessoaResponse>(
            itens.Select(x => PessoaResponse.De(x)).ToList(), p.Pagina, p.TamanhoPagina, total);
    }

    public async Task<PessoaResponse> AtualizarAsync(Guid id, PessoaRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var pessoa = await ObterOuFalharAsync(id, cancellationToken);

        var cpf = Cpf.Normalizar(request.Cpf);
        if (cpf != pessoa.Cpf)
        {
            await GarantirCpfUnicoAsync(cpf, id, cancellationToken);
            pessoa.AlterarCpf(cpf);
        }

        pessoa.Atualizar(request.Nome, request.DataNascimento, request.Email, request.Telefone);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return PessoaResponse.De(pessoa);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pessoa = await ObterOuFalharAsync(id, cancellationToken);

        var quantidadeVeiculos = await pessoas.ContarVeiculosAsync(id, cancellationToken);
        if (quantidadeVeiculos > 0)
            throw new ConflictException(
                $"Não é possível excluir a pessoa: há {quantidadeVeiculos} veículo(s) vinculado(s). " +
                "Exclua ou transfira os veículos antes.");

        pessoas.Remover(pessoa);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Pessoa> ObterOuFalharAsync(Guid id, CancellationToken cancellationToken) =>
        await pessoas.ObterPorIdAsync(id, cancellationToken)
        ?? throw new NotFoundException($"Pessoa '{id}' não encontrada.");

    private async Task GarantirCpfUnicoAsync(string cpf, Guid? ignorarId, CancellationToken cancellationToken)
    {
        if (await pessoas.CpfJaCadastradoAsync(cpf, ignorarId, cancellationToken))
            throw new ConflictException($"Já existe uma pessoa cadastrada com o CPF {Cpf.Formatar(cpf)}.");
    }
}
