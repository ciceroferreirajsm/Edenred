namespace Cadastro.Domain.Validation;

/// <summary>
/// Utilitários para normalização e validação de CPF (incluindo dígitos verificadores).
/// </summary>
public static class Cpf
{
    /// <summary>Remove qualquer caractere que não seja dígito ("123.456.789-09" → "12345678909").</summary>
    public static string Normalizar(string? cpf) =>
        cpf is null ? string.Empty : new string(cpf.Where(char.IsDigit).ToArray());

    public static bool EhValido(string? cpf)
    {
        var digitos = Normalizar(cpf);

        if (digitos.Length != 11)
            return false;

        // Sequências repetidas (000.000.000-00, 111..., etc.) passam no cálculo mas são inválidas.
        if (digitos.All(c => c == digitos[0]))
            return false;

        var numeros = digitos.Select(c => c - '0').ToArray();

        return numeros[9] == CalcularDigito(numeros, 9) &&
               numeros[10] == CalcularDigito(numeros, 10);
    }

    /// <summary>Formata como 000.000.000-00 (assume entrada válida).</summary>
    public static string Formatar(string cpf)
    {
        var d = Normalizar(cpf);
        return d.Length == 11 ? $"{d[..3]}.{d[3..6]}.{d[6..9]}-{d[9..]}" : cpf;
    }

    private static int CalcularDigito(int[] numeros, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += numeros[i] * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
