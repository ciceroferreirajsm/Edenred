using Cadastro.Domain.Entities;

namespace Cadastro.Application.Veiculos;

/// <summary>Dados para criar/atualizar um veículo.</summary>
/// <param name="Placa">Placa no padrão antigo (ABC1234) ou Mercosul (ABC1D23); hífen opcional.</param>
/// <param name="AnoFabricacao">Ano de fabricação (não pode ser maior que o ano atual).</param>
/// <param name="Marca">Marca informada manualmente.</param>
/// <param name="Modelo">Modelo informado manualmente.</param>
/// <param name="PessoaId">Pessoa proprietária do veículo.</param>
public sealed record VeiculoRequest(
    string Placa,
    int AnoFabricacao,
    string Marca,
    string Modelo,
    Guid PessoaId);

/// <summary>Vincula/transfere o veículo para uma pessoa.</summary>
/// <param name="PessoaId">Nova pessoa proprietária.</param>
public sealed record VincularVeiculoRequest(Guid PessoaId);

public sealed record VeiculoResponse(
    Guid Id,
    string Placa,
    int AnoFabricacao,
    string Marca,
    string Modelo,
    Guid PessoaId,
    string? PessoaNome,
    DateTime DataCadastro,
    DateTime? DataAtualizacao)
{
    public static VeiculoResponse De(Veiculo v, string? pessoaNome = null) => new(
        v.Id, v.Placa, v.AnoFabricacao, v.Marca, v.Modelo, v.PessoaId,
        pessoaNome ?? v.Pessoa?.Nome, v.DataCadastro, v.DataAtualizacao);
}
