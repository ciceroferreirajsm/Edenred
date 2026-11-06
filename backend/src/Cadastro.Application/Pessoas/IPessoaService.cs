using Cadastro.Application.Common;

namespace Cadastro.Application.Pessoas;

public interface IPessoaService
{
    Task<PessoaResponse> CriarAsync(PessoaRequest request, CancellationToken cancellationToken = default);
    Task<PessoaResponse> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<PessoaResponse>> ListarAsync(string? busca, Paginacao paginacao, CancellationToken cancellationToken = default);
    Task<PessoaResponse> AtualizarAsync(Guid id, PessoaRequest request, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}
