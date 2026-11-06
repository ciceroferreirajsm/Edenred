using System.Net;
using System.Net.Http.Json;
using Cadastro.Application.Common;
using Cadastro.Application.Pessoas;
using Cadastro.IntegrationTests.Support;
using Microsoft.AspNetCore.Mvc;

namespace Cadastro.IntegrationTests;

public class PessoasEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static PessoaRequest NovaPessoa(string? cpf = null) =>
        new("Maria Silva", cpf ?? Dados.CpfValido(), new DateOnly(1990, 5, 20), "maria@email.com", "(11) 98765-4321");

    [Fact]
    public async Task Post_DeveCriarPessoa_ERetornar201ComLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/pessoas", NovaPessoa());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var criada = await response.Content.ReadFromJsonAsync<PessoaResponse>();
        var consultada = await _client.GetFromJsonAsync<PessoaResponse>(response.Headers.Location);

        Assert.Equal(criada!.Id, consultada!.Id);
        Assert.Equal("11987654321", consultada.Telefone);
    }

    [Fact]
    public async Task Post_DeveRetornar409_QuandoCpfDuplicado()
    {
        var cpf = Dados.CpfValido();
        (await _client.PostAsJsonAsync("/api/pessoas", NovaPessoa(cpf))).EnsureSuccessStatusCode();

        var cpfComMascara = $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";
        var response = await _client.PostAsJsonAsync("/api/pessoas", NovaPessoa(cpfComMascara));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Contains("CPF", problem!.Detail);
    }

    [Fact]
    public async Task Post_DeveRetornar400ComErrosPorCampo_QuandoDadosInvalidos()
    {
        var request = NovaPessoa() with { Nome = "", Email = "email-invalido", Cpf = "123" };

        var response = await _client.PostAsJsonAsync("/api/pessoas", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("nome", problem!.Errors.Keys);
        Assert.Contains("email", problem.Errors.Keys);
        Assert.Contains("cpf", problem.Errors.Keys);
    }

    [Fact]
    public async Task Post_DeveRetornar400_QuandoNomeAusenteNoJson()
    {
        var response = await _client.PostAsJsonAsync("/api/pessoas",
            new { cpf = Dados.CpfValido(), dataNascimento = "1990-01-01", email = "a@b.com" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(["Nome é obrigatório."], problem!.Errors["nome"]);
    }

    [Fact]
    public async Task Get_DeveRetornar404_QuandoPessoaNaoExiste()
    {
        var response = await _client.GetAsync($"/api/pessoas/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_DeveAtualizarPessoa()
    {
        var criada = await (await _client.PostAsJsonAsync("/api/pessoas", NovaPessoa()))
            .Content.ReadFromJsonAsync<PessoaResponse>();

        var atualizacao = new PessoaRequest("Maria Souza", criada!.Cpf, criada.DataNascimento, "nova@email.com", null);
        var response = await _client.PutAsJsonAsync($"/api/pessoas/{criada.Id}", atualizacao);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var atualizada = await response.Content.ReadFromJsonAsync<PessoaResponse>();
        Assert.Equal("Maria Souza", atualizada!.Nome);
        Assert.Equal("nova@email.com", atualizada.Email);
        Assert.NotNull(atualizada.DataAtualizacao);
    }

    [Fact]
    public async Task GetLista_DeveFiltrarPorBuscaEPaginar()
    {
        var nomeUnico = $"Pessoa Busca {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/pessoas", NovaPessoa() with { Nome = nomeUnico });

        var result = await _client.GetFromJsonAsync<PagedResult<PessoaResponse>>(
            $"/api/pessoas?busca={nomeUnico[^10..]}&pagina=1&tamanhoPagina=5");

        Assert.Equal(1, result!.TotalItens);
        Assert.Equal(nomeUnico, result.Itens[0].Nome);
    }

    [Fact]
    public async Task Delete_DeveExcluirPessoaSemVeiculos()
    {
        var criada = await (await _client.PostAsJsonAsync("/api/pessoas", NovaPessoa()))
            .Content.ReadFromJsonAsync<PessoaResponse>();

        var delete = await _client.DeleteAsync($"/api/pessoas/{criada!.Id}");
        var get = await _client.GetAsync($"/api/pessoas/{criada.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
