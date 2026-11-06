using Cadastro.Application.Common;
using Cadastro.Application.Pessoas;
using Cadastro.Application.Veiculos;
using Microsoft.AspNetCore.Mvc;

namespace Cadastro.Api.Controllers;

[ApiController]
[Route("api/pessoas")]
[Produces("application/json")]
public sealed class PessoasController(IPessoaService pessoaService, IVeiculoService veiculoService) : ControllerBase
{
    /// <summary>Lista pessoas com paginação e busca opcional por nome, e-mail ou CPF.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<PessoaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PessoaResponse>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await pessoaService.ListarAsync(busca, new Paginacao(pagina, tamanhoPagina), cancellationToken));
    }

    /// <summary>Consulta uma pessoa pelo identificador.</summary>
    [HttpGet("{id:guid}", Name = nameof(ObterPessoa))]
    [ProducesResponseType<PessoaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PessoaResponse>> ObterPessoa(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await pessoaService.ObterAsync(id, cancellationToken));
    }

    /// <summary>Lista os veículos vinculados a uma pessoa.</summary>
    [HttpGet("{id:guid}/veiculos")]
    [ProducesResponseType<PagedResult<VeiculoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<VeiculoResponse>>> ListarVeiculos(
        Guid id,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 50,
        CancellationToken cancellationToken = default)
    {
        await pessoaService.ObterAsync(id, cancellationToken); // 404 se a pessoa não existir
        return Ok(await veiculoService.ListarAsync(null, id, new Paginacao(pagina, tamanhoPagina), cancellationToken));
    }

    /// <summary>Cadastra uma nova pessoa.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<PessoaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PessoaResponse>> Criar(PessoaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = await pessoaService.CriarAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(ObterPessoa), new { id = pessoa.Id }, pessoa);
    }

    /// <summary>Atualiza os dados de uma pessoa.</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType<PessoaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PessoaResponse>> Atualizar(Guid id, PessoaRequest request, CancellationToken cancellationToken)
    {
        return Ok(await pessoaService.AtualizarAsync(id, request, cancellationToken));
    }

    /// <summary>Exclui uma pessoa (somente se não houver veículos vinculados).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await pessoaService.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }
}
