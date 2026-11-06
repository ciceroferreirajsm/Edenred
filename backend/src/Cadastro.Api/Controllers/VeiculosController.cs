using Cadastro.Application.Common;
using Cadastro.Application.Veiculos;
using Microsoft.AspNetCore.Mvc;

namespace Cadastro.Api.Controllers;

[ApiController]
[Route("api/veiculos")]
[Produces("application/json")]
public sealed class VeiculosController(IVeiculoService veiculoService) : ControllerBase
{
    /// <summary>Lista veículos com paginação, busca por placa/marca/modelo e filtro opcional por pessoa.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<VeiculoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<VeiculoResponse>>> Listar(
        [FromQuery] string? busca,
        [FromQuery] Guid? pessoaId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        return Ok(await veiculoService.ListarAsync(busca, pessoaId, new Paginacao(pagina, tamanhoPagina), cancellationToken));
    }

    /// <summary>Consulta um veículo pelo identificador.</summary>
    [HttpGet("{id:guid}", Name = nameof(ObterVeiculo))]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeiculoResponse>> ObterVeiculo(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await veiculoService.ObterAsync(id, cancellationToken));
    }

    /// <summary>Cadastra um novo veículo vinculado a uma pessoa.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VeiculoResponse>> Criar(VeiculoRequest request, CancellationToken cancellationToken)
    {
        var veiculo = await veiculoService.CriarAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(ObterVeiculo), new { id = veiculo.Id }, veiculo);
    }

    /// <summary>Atualiza os dados de um veículo.</summary>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VeiculoResponse>> Atualizar(Guid id, VeiculoRequest request, CancellationToken cancellationToken)
    {
        return Ok(await veiculoService.AtualizarAsync(id, request, cancellationToken));
    }

    /// <summary>Vincula (ou transfere) o veículo a uma pessoa.</summary>
    [HttpPatch("{id:guid}/pessoa")]
    [Consumes("application/json")]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeiculoResponse>> VincularPessoa(Guid id, VincularVeiculoRequest request, CancellationToken cancellationToken)
    {
        return Ok(await veiculoService.VincularPessoaAsync(id, request, cancellationToken));
    }

    /// <summary>Exclui um veículo.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await veiculoService.ExcluirAsync(id, cancellationToken);
        return NoContent();
    }
}
