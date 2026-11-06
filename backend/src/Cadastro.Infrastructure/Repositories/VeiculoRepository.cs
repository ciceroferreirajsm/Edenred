using Cadastro.Application.Abstractions;
using Cadastro.Domain.Entities;
using Cadastro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cadastro.Infrastructure.Repositories;

public sealed class VeiculoRepository(AppDbContext context) : IVeiculoRepository
{
    public Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Veiculos
            .Include(v => v.Pessoa)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<bool> PlacaJaCadastradaAsync(string placa, Guid? ignorarId = null, CancellationToken cancellationToken = default) =>
        context.Veiculos.AnyAsync(v => v.Placa == placa && (ignorarId == null || v.Id != ignorarId), cancellationToken);

    public async Task<(IReadOnlyList<Veiculo> Itens, int Total)> ListarAsync(
        string? busca, Guid? pessoaId, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var query = context.Veiculos.AsNoTracking();

        if (pessoaId.HasValue)
            query = query.Where(v => v.PessoaId == pessoaId.Value);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            var placa = new string(busca.Where(char.IsLetterOrDigit).ToArray()).ToUpper();

            query = query.Where(v =>
                (placa.Length > 0 && v.Placa.Contains(placa)) ||
                v.Marca.ToLower().Contains(termo) ||
                v.Modelo.ToLower().Contains(termo));
        }

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .Include(v => v.Pessoa)
            .OrderBy(v => v.Placa)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public void Adicionar(Veiculo veiculo) => context.Veiculos.Add(veiculo);

    public void Remover(Veiculo veiculo) => context.Veiculos.Remove(veiculo);
}
