using Cadastro.Application.Common;
using Cadastro.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Cadastro.Api.Middlewares;

/// <summary>
/// Converte exceções em respostas HTTP padronizadas (RFC 9457 - Problem Details).
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const int SqliteConstraintError = 19;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException ex => CriarValidationProblem(ex),
            DomainException ex => Criar(StatusCodes.Status400BadRequest, "Regra de negócio violada", ex.Message),
            NotFoundException ex => Criar(StatusCodes.Status404NotFound, "Recurso não encontrado", ex.Message),
            ConflictException ex => Criar(StatusCodes.Status409Conflict, "Conflito", ex.Message),
            DbUpdateException { InnerException: SqliteException { SqliteErrorCode: SqliteConstraintError } } =>
                Criar(StatusCodes.Status409Conflict, "Conflito",
                    "A operação viola uma restrição de integridade (registro duplicado ou vinculado)."),
            _ => null
        };

        if (problem is null)
        {
            logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            problem = Criar(StatusCodes.Status500InternalServerError, "Erro interno",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }
        else
        {
            logger.LogInformation("Requisição rejeitada ({Status}): {Mensagem}", problem.Status, exception.Message);
        }

        httpContext.Response.StatusCode = problem.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    private static ProblemDetails Criar(int status, string titulo, string detalhe) => new()
    {
        Status = status,
        Title = titulo,
        Detail = detalhe
    };

    private static ValidationProblemDetails CriarValidationProblem(ValidationException ex)
    {
        var erros = ex.Errors
            .GroupBy(e => ToCamelCase(e.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

        return new ValidationProblemDetails(erros)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Dados inválidos",
            Detail = "Um ou mais campos possuem erros de validação."
        };
    }

    private static string ToCamelCase(string nome) =>
        string.IsNullOrEmpty(nome) ? nome : char.ToLowerInvariant(nome[0]) + nome[1..];
}
