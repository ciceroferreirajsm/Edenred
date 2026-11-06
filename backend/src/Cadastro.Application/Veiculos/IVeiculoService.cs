using Cadastro.Application.Common;

namespace Cadastro.Application.Veiculos;

public interface IVeiculoService
{
    Task<VeiculoResponse> CriarAsync(VeiculoRequest request, CancellationToken cancellationToken = default);
    Task<VeiculoResponse> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<VeiculoResponse>> ListarAsync(string? busca, Guid? pessoaId, Paginacao paginacao, CancellationToken cancellationToken = default);
    Task<VeiculoResponse> AtualizarAsync(Guid id, VeiculoRequest request, CancellationToken cancellationToken = default);
    Task<VeiculoResponse> VincularPessoaAsync(Guid id, VincularVeiculoRequest request, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}
