using Cadastro.Domain.Entities;

namespace Cadastro.Application.Abstractions;

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> PlacaJaCadastradaAsync(string placa, Guid? ignorarId = null, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Veiculo> Itens, int Total)> ListarAsync(string? busca, Guid? pessoaId, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);
    void Adicionar(Veiculo veiculo);
    void Remover(Veiculo veiculo);
}
