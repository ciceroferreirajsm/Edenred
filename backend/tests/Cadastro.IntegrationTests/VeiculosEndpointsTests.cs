using System.Net;
using System.Net.Http.Json;
using Cadastro.Application.Common;
using Cadastro.Application.Pessoas;
using Cadastro.Application.Veiculos;
using Cadastro.IntegrationTests.Support;
using Microsoft.AspNetCore.Mvc;

namespace Cadastro.IntegrationTests;

public class VeiculosEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<PessoaResponse> CriarPessoaAsync(string nome = "Dono Teste")
    {
        var request = new PessoaRequest(nome, Dados.CpfValido(), new DateOnly(1980, 1, 1), "dono@email.com", null);
        var response = await _client.PostAsJsonAsync("/api/pessoas", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PessoaResponse>())!;
    }

    private async Task<VeiculoResponse> CriarVeiculoAsync(Guid pessoaId, string? placa = null)
    {
        var response = await _client.PostAsJsonAsync("/api/veiculos",
            new VeiculoRequest(placa ?? Dados.PlacaValida(), 2020, "Fiat", "Argo", pessoaId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VeiculoResponse>())!;
    }

    [Fact]
    public async Task Post_DeveCriarVeiculoVinculadoAPessoa()
    {
        var pessoa = await CriarPessoaAsync("Carlos Dono");

        var response = await _client.PostAsJsonAsync("/api/veiculos",
            new VeiculoRequest("qwe-1d23", 2020, "Fiat", "Argo", pessoa.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var veiculo = await response.Content.ReadFromJsonAsync<VeiculoResponse>();
        Assert.Equal("QWE1D23", veiculo!.Placa);
        Assert.Equal(pessoa.Id, veiculo.PessoaId);
        Assert.Equal("Carlos Dono", veiculo.PessoaNome);
    }

    [Fact]
    public async Task Post_DeveRetornar409_QuandoPlacaDuplicada()
    {
        var pessoa = await CriarPessoaAsync();
        var placa = Dados.PlacaValida();
        await CriarVeiculoAsync(pessoa.Id, placa);

        var response = await _client.PostAsJsonAsync("/api/veiculos",
            new VeiculoRequest(placa.ToLower(), 2019, "VW", "Gol", pessoa.Id));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Post_DeveRetornar400_QuandoAnoMaiorQueAnoAtual()
    {
        var pessoa = await CriarPessoaAsync();

        var response = await _client.PostAsJsonAsync("/api/veiculos",
            new VeiculoRequest(Dados.PlacaValida(), DateTime.Now.Year + 1, "VW", "Gol", pessoa.Id));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("anoFabricacao", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Post_DeveRetornar404_QuandoPessoaNaoExiste()
    {
        var response = await _client.PostAsJsonAsync("/api/veiculos",
            new VeiculoRequest(Dados.PlacaValida(), 2020, "VW", "Gol", Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FluxoCompleto_VincularTransferirEExcluir()
    {
        var joao = await CriarPessoaAsync("João");
        var ana = await CriarPessoaAsync("Ana");
        var veiculo = await CriarVeiculoAsync(joao.Id);

        // Pessoa com veículo não pode ser excluída
        var deleteJoao = await _client.DeleteAsync($"/api/pessoas/{joao.Id}");
        Assert.Equal(HttpStatusCode.Conflict, deleteJoao.StatusCode);

        // Contagem de veículos e listagem por pessoa
        var joaoConsultado = await _client.GetFromJsonAsync<PessoaResponse>($"/api/pessoas/{joao.Id}");
        Assert.Equal(1, joaoConsultado!.QuantidadeVeiculos);

        var veiculosJoao = await _client.GetFromJsonAsync<PagedResult<VeiculoResponse>>($"/api/pessoas/{joao.Id}/veiculos");
        Assert.Single(veiculosJoao!.Itens);

        // Transferência (vinculação) para outra pessoa
        var patch = await _client.PatchAsJsonAsync($"/api/veiculos/{veiculo.Id}/pessoa", new VincularVeiculoRequest(ana.Id));
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);
        var transferido = await patch.Content.ReadFromJsonAsync<VeiculoResponse>();
        Assert.Equal(ana.Id, transferido!.PessoaId);
        Assert.Equal("Ana", transferido.PessoaNome);

        // Agora João pode ser excluído
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/pessoas/{joao.Id}")).StatusCode);

        // Atualização do veículo
        var put = await _client.PutAsJsonAsync($"/api/veiculos/{veiculo.Id}",
            new VeiculoRequest(veiculo.Placa, 2021, "Fiat", "Pulse", ana.Id));
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        // Exclusão do veículo
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/veiculos/{veiculo.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/veiculos/{veiculo.Id}")).StatusCode);
    }

    [Fact]
    public async Task GetLista_DeveFiltrarPorPessoa()
    {
        var pessoa = await CriarPessoaAsync();
        await CriarVeiculoAsync(pessoa.Id);
        await CriarVeiculoAsync(pessoa.Id);

        var result = await _client.GetFromJsonAsync<PagedResult<VeiculoResponse>>($"/api/veiculos?pessoaId={pessoa.Id}");

        Assert.Equal(2, result!.TotalItens);
        Assert.All(result.Itens, v => Assert.Equal(pessoa.Id, v.PessoaId));
    }
}
