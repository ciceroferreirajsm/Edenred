namespace Cadastro.IntegrationTests.Support;

/// <summary>Gera CPFs e placas válidos e únicos para que os testes não colidam entre si.</summary>
public static class Dados
{
    private static readonly Random Rnd = new();

    public static string CpfValido()
    {
        int[] n;
        do
        {
            n = Enumerable.Range(0, 9).Select(_ => Rnd.Next(10)).ToArray();
        } while (n.All(d => d == n[0]));

        var d1 = Digito(n);
        var d2 = Digito([.. n, d1]);
        return string.Concat(n) + d1 + d2;
    }

    public static string PlacaValida()
    {
        var letras = new string(Enumerable.Range(0, 3).Select(_ => (char)('A' + Rnd.Next(26))).ToArray());
        return $"{letras}{Rnd.Next(10)}{(char)('A' + Rnd.Next(10))}{Rnd.Next(10, 100)}";
    }

    private static int Digito(int[] numeros)
    {
        var peso = numeros.Length + 1;
        var soma = numeros.Select((d, i) => d * (peso - i)).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
