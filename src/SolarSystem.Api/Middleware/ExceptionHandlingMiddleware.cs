using Microsoft.AspNetCore.Mvc;
using SolarSystem.Domain.Common;
using ValidationException = SolarSystem.Application.Common.Exceptions.ValidationException;

namespace SolarSystem.Api.Middleware;

/// <summary>
/// Ponto unico de traducao de excecao para resposta HTTP. Garante que nenhuma stack trace
/// vaze para o cliente, independente do ambiente.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteAsync(context, new ValidationProblemDetails(ex.Errors.ToDictionary(e => e.Key, e => e.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro de validação."
            });
        }
        catch (DomainException ex)
        {
            await WriteAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Regra de negócio violada.",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);

            await WriteAsync(context, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno.",
                Detail = "Ocorreu um erro inesperado. Tente novamente."
            });
        }
    }

    private static async Task WriteAsync(HttpContext context, ProblemDetails problem)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, problem.GetType());
    }
}
