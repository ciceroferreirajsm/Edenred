namespace Cadastro.Application.Common;

public sealed record PagedResult<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, int TotalItens)
{
    public int TotalPaginas => TamanhoPagina <= 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);
}

public sealed record Paginacao(int Pagina = 1, int TamanhoPagina = 10)
{
    public const int TamanhoMaximo = 100;

    /// <summary>Garante valores dentro dos limites aceitos (página ≥ 1, tamanho entre 1 e 100).</summary>
    public Paginacao Normalizar() =>
        new(Math.Max(1, Pagina), Math.Clamp(TamanhoPagina, 1, TamanhoMaximo));
}
