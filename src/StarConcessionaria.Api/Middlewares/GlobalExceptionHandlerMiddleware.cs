using System.Net;
using System.Text.Json;
using StarConcessionaria.Domain.Exceptions;

namespace StarConcessionaria.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ocorrido na requisição: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title) = exception switch
        {
            VeiculoIndisponivelException => (HttpStatusCode.Conflict, "Veículo Indisponível"),
            ConflitoAgendamentoException => (HttpStatusCode.Conflict, "Conflito de Agendamento"),
            DomainException => (HttpStatusCode.BadRequest, "Violação de Regra de Negócio"),
            _ => (HttpStatusCode.InternalServerError, "Erro Interno no Servidor")
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title = title,
            status = (int)statusCode,
            detail = exception.Message,
            instance = context.Request.Path.Value
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}
