using System.Text.RegularExpressions;

namespace Cadastro.Domain.Validation;

/// <summary>
/// Normalização e validação de placas brasileiras: padrão antigo (ABC1234) e Mercosul (ABC1D23).
/// </summary>
public static partial class Placa
{
    [GeneratedRegex("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$")]
    private static partial Regex FormatoPlaca();

    /// <summary>Remove hífen/espaços e converte para maiúsculas ("abc-1234" → "ABC1234").</summary>
    public static string Normalizar(string? placa) =>
        placa is null
            ? string.Empty
            : new string(placa.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

    public static bool EhValida(string? placa) => FormatoPlaca().IsMatch(Normalizar(placa));
}
