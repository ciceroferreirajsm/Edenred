using Cadastro.Application.Abstractions;
using Cadastro.Domain.Entities;
using Cadastro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cadastro.Infrastructure.Repositories;

public sealed class PessoaRepository(AppDbContext context) : IPessoaRepository
{
    public Task<Pessoa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Pessoas
            .Include(p => p.Veiculos)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> CpfJaCadastradoAsync(string cpf, Guid? ignorarId = null, CancellationToken cancellationToken = default) =>
        context.Pessoas.AnyAsync(p => p.Cpf == cpf && (ignorarId == null || p.Id != ignorarId), cancellationToken);

    public async Task<(IReadOnlyList<Pessoa> Itens, int Total)> ListarAsync(
        string? busca, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var query = context.Pessoas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            var digitos = new string(busca.Where(char.IsDigit).ToArray());

            query = query.Where(p =>
                p.Nome.ToLower().Contains(termo) ||
                p.Email.Contains(termo) ||
                (digitos.Length > 0 && p.Cpf.Contains(digitos)));
        }

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .Include(p => p.Veiculos)
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return (itens, total);
    }

    public Task<int> ContarVeiculosAsync(Guid pessoaId, CancellationToken cancellationToken = default) =>
        context.Veiculos.CountAsync(v => v.PessoaId == pessoaId, cancellationToken);

    public void Adicionar(Pessoa pessoa) => context.Pessoas.Add(pessoa);

    public void Remover(Pessoa pessoa) => context.Pessoas.Remove(pessoa);
}
