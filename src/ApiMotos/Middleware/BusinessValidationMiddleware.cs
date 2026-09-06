// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - Middleware HTTP
// Maneja 3 tipos de errores:
// 1. FluentValidation (entrada) -> 400 Bad Request
// 2. DomainException (reglas de negocio) -> 422 Unprocessable Entity
// 3. Exception general -> 500 Internal Server Error
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text.Json;
using FluentValidation;
using ApiMotos.Application.Common.Behaviors;
using ApiMotos.Application.Common.Validation;
using ApiMotos.Domain.Exceptions;
using ApiMotos.Domain.Specifications;
using ApiMotos.Shared.Exceptions;

namespace ApiMotos.Middleware;

/// <summary>
/// Middleware que captura y mapea excepciones a respuestas HTTP apropiadas
/// </summary>
public class BusinessValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BusinessValidationMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public BusinessValidationMiddleware(RequestDelegate next, ILogger<BusinessValidationMiddleware> logger)
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
        catch (SecurityException ex)
        {
            // Security errors -> 401 Unauthorized / 403 Forbidden
            _logger.LogWarning("[Security] {ErrorCode}: {Message}. Capability: {Capability}",
                ex.ErrorCode, ex.Message, ex.RequiredCapability);
            await HandleSecurityException(context, ex);
        }
        catch (ValidationException ex)
        {
            // FluentValidation errors -> 400 Bad Request
            _logger.LogWarning("Input validation failed: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            await HandleFluentValidationException(context, ex);
        }
        catch (DomainException ex)
        {
            // Domain business rule errors -> 422 Unprocessable Entity
            _logger.LogWarning("Domain validation failed [{Code}]: {Message}",
                ex.ErrorCode.Name, ex.Message);
            await HandleDomainException(context, ex);
        }
        catch (BusinessValidationException ex)
        {
            // Legacy business validation (mantener compatibilidad)
            _logger.LogWarning("Business validation failed for {Entity}.{Action}: {Message}",
                ex.EntityName, ex.ActionName, ex.Message);
            await HandleBusinessValidationException(context, ex);
        }
    }

    /// <summary>
    /// Security errors -> HTTP 401 Unauthorized / 403 Forbidden
    /// Errores de autenticación y autorización
    /// </summary>
    private async Task HandleSecurityException(HttpContext context, SecurityException ex)
    {
        // Usar el valor numérico del enum directamente (401 o 403)
        var statusCode = (int)ex.ErrorCode;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var response = new ProblemDetailsResponse
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = statusCode == 401 ? "Not Authenticated" : "Access Denied",
            Status = statusCode,
            Detail = ex.Message,
            Instance = context.Request.Path,
            Errors = new List<ValidationErrorDto>
            {
                new()
                {
                    Code = ex.ErrorCode.ToString(),
                    Message = ex.Message,
                    Field = ex.RequiredCapability
                }
            },
            TraceId = context.TraceIdentifier
        };

        await WriteJsonResponse(context, response);
    }

    /// <summary>
    /// FluentValidation errors -> HTTP 400 Bad Request
    /// Errores de formato/sintaxis en los datos de entrada
    /// </summary>
    private async Task HandleFluentValidationException(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";

        var response = new ProblemDetailsResponse
        {
            Type = "https://httpstatuses.io/400",
            Title = "Input Validation Failed",
            Status = 400,
            Detail = "One or more validation errors occurred with the input data.",
            Instance = context.Request.Path,
            Errors = ex.Errors.Select(e => new ValidationErrorDto
            {
                Code = e.ErrorCode ?? "VALIDATION_ERROR",
                Message = e.ErrorMessage,
                Field = e.PropertyName
            }).ToList(),
            TraceId = context.TraceIdentifier
        };

        await WriteJsonResponse(context, response);
    }

    /// <summary>
    /// Domain exceptions -> HTTP 422 Unprocessable Entity
    /// Errores de reglas de negocio/dominio
    /// </summary>
    private async Task HandleDomainException(HttpContext context, DomainException ex)
    {
        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        context.Response.ContentType = "application/problem+json";

        var response = new ProblemDetailsResponse
        {
            Type = "https://httpstatuses.io/422",
            Title = "Business Rule Violation",
            Status = 422,
            Detail = ex.Message,
            Instance = context.Request.Path,
            Errors = new List<ValidationErrorDto>
            {
                new()
                {
                    Code = ex.ErrorCode.Name,
                    NumericCode = ex.ErrorCode.Code,
                    Category = ex.ErrorCode.Category.ToString(),
                    Message = ex.Message,
                    Field = ex.FieldName
                }
            },
            TraceId = context.TraceIdentifier
        };

        await WriteJsonResponse(context, response);
    }

    /// <summary>
    /// Legacy BusinessValidationException -> HTTP 422
    /// Mantener compatibilidad con codigo existente
    /// </summary>
    private async Task HandleBusinessValidationException(HttpContext context, BusinessValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        context.Response.ContentType = "application/problem+json";

        var response = new ProblemDetailsResponse
        {
            Type = "https://httpstatuses.io/422",
            Title = "Business Validation Failed",
            Status = 422,
            Detail = ex.Message,
            Instance = context.Request.Path,
            Entity = ex.EntityName,
            Action = ex.ActionName,
            Errors = ex.Errors.Select(e => new ValidationErrorDto
            {
                Code = e.Code ?? "VALIDATION_ERROR",
                Message = e.Message,
                Field = e.Field
            }).ToList(),
            TraceId = context.TraceIdentifier
        };

        await WriteJsonResponse(context, response);
    }

    private static async Task WriteJsonResponse(HttpContext context, object response)
    {
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Respuesta de error siguiendo RFC 7807 Problem Details
/// </summary>
public class ProblemDetailsResponse
{
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public int Status { get; set; }
    public string Detail { get; set; } = "";
    public string Instance { get; set; } = "";
    public string? Entity { get; set; }
    public string? Action { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public string TraceId { get; set; } = "";
}

public class ValidationErrorDto
{
    /// <summary>Codigo de error (string, ej: "MUST_BE_DRAFT")</summary>
    public string Code { get; set; } = "";

    /// <summary>Codigo numerico para logica en frontend (ej: 1001)</summary>
    public int? NumericCode { get; set; }

    /// <summary>Categoria del error (State, Temporal, Dependency, etc)</summary>
    public string? Category { get; set; }

    /// <summary>Mensaje legible para el usuario</summary>
    public string Message { get; set; } = "";

    /// <summary>Campo relacionado con el error</summary>
    public string? Field { get; set; }

    /// <summary>ID del validador (legacy)</summary>
    public string? ValidatorId { get; set; }
}

/// <summary>
/// Extension methods para registrar el middleware
/// </summary>
public static class BusinessValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseBusinessValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<BusinessValidationMiddleware>();
    }
}
