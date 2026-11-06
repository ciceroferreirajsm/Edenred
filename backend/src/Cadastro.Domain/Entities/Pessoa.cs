using Cadastro.Domain.Exceptions;
using Cadastro.Domain.Validation;

namespace Cadastro.Domain.Entities;

public class Pessoa
{
    private readonly List<Veiculo> _veiculos = [];

    // Construtor exigido pelo EF Core.
    private Pessoa() { }

    public Pessoa(string nome, string cpf, DateOnly dataNascimento, string email, string? telefone)
    {
        Id = Guid.NewGuid();
        Cpf = NormalizarCpf(cpf);
        DataCadastro = DateTime.UtcNow;
        Atualizar(nome, dataNascimento, email, telefone);
        DataAtualizacao = null;
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public DateOnly DataNascimento { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    public IReadOnlyCollection<Veiculo> Veiculos => _veiculos.AsReadOnly();

    public void Atualizar(string nome, DateOnly dataNascimento, string email, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("E-mail é obrigatório.");

        Nome = nome.Trim();
        DataNascimento = dataNascimento;
        Email = email.Trim().ToLowerInvariant();
        Telefone = NormalizarTelefone(telefone);
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AlterarCpf(string cpf)
    {
        Cpf = NormalizarCpf(cpf);
        DataAtualizacao = DateTime.UtcNow;
    }

    private static string NormalizarCpf(string cpf)
    {
        if (!Validation.Cpf.EhValido(cpf))
            throw new DomainException("CPF inválido.");

        return Validation.Cpf.Normalizar(cpf);
    }

    private static string? NormalizarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return null;

        var digitos = new string(telefone.Where(char.IsDigit).ToArray());
        return digitos.Length == 0 ? null : digitos;
    }
}
