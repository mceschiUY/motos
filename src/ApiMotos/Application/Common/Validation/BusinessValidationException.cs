// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - Excepción de Validación
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Application.Common.Validation;

/// <summary>
/// Excepción lanzada cuando falla una validación de negocio
/// Se mapea automáticamente a HTTP 422 (Unprocessable Entity)
/// </summary>
public class BusinessValidationException : Exception
{
    public List<BusinessValidationError> Errors { get; }
    public string EntityName { get; }
    public string ActionName { get; }

    public BusinessValidationException(
        string message,
        string? entityName = null,
        string? actionName = null)
        : base(message)
    {
        Errors = new List<BusinessValidationError> { new(message) };
        EntityName = entityName ?? "Unknown";
        ActionName = actionName ?? "Unknown";
    }

    public BusinessValidationException(
        IEnumerable<BusinessValidationError> errors,
        string? entityName = null,
        string? actionName = null)
        : base(errors.FirstOrDefault()?.Message ?? "Validation failed")
    {
        Errors = errors.ToList();
        EntityName = entityName ?? "Unknown";
        ActionName = actionName ?? "Unknown";
    }

    public BusinessValidationException(
        BusinessValidationResult result,
        string? entityName = null,
        string? actionName = null)
        : base(result.Errors.FirstOrDefault()?.Message ?? "Validation failed")
    {
        Errors = result.Errors;
        EntityName = entityName ?? "Unknown";
        ActionName = actionName ?? "Unknown";
    }
}
