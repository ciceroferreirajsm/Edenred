using Cadastro.Domain.Entities;

namespace Cadastro.Application.Abstractions;

public interface IPessoaRepository
{
    Task<Pessoa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CpfJaCadastradoAsync(string cpf, Guid? ignorarId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Pessoa> Itens, int Total)> ListarAsync(string? busca, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);
    Task<int> ContarVeiculosAsync(Guid pessoaId, CancellationToken cancellationToken = default);
    void Adicionar(Pessoa pessoa);
    void Remover(Pessoa pessoa);
}
