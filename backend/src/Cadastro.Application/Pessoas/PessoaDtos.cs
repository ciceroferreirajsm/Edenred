using Cadastro.Domain.Entities;

namespace Cadastro.Application.Pessoas;

/// <summary>Dados para criar/atualizar uma pessoa.</summary>
/// <param name="Nome">Nome completo (obrigatório).</param>
/// <param name="Cpf">CPF com ou sem máscara (único).</param>
/// <param name="DataNascimento">Data de nascimento (yyyy-MM-dd).</param>
/// <param name="Email">E-mail em formato válido.</param>
/// <param name="Telefone">Telefone com DDD, com ou sem máscara (opcional).</param>
public sealed record PessoaRequest(
    string Nome,
    string Cpf,
    DateOnly DataNascimento,
    string Email,
    string? Telefone);

public sealed record PessoaResponse(
    Guid Id,
    string Nome,
    string Cpf,
    DateOnly DataNascimento,
    string Email,
    string? Telefone,
    int QuantidadeVeiculos,
    DateTime DataCadastro,
    DateTime? DataAtualizacao)
{
    public static PessoaResponse De(Pessoa p, int? quantidadeVeiculos = null) => new(
        p.Id, p.Nome, p.Cpf, p.DataNascimento, p.Email, p.Telefone,
        quantidadeVeiculos ?? p.Veiculos.Count, p.DataCadastro, p.DataAtualizacao);
}
