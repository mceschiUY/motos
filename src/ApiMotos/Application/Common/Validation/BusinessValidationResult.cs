// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - Resultado de Validación
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Application.Common.Validation;

/// <summary>
/// Representa el resultado de una validación de negocio
/// </summary>
public class BusinessValidationResult
{
    public bool IsValid { get; private set; }
    public List<BusinessValidationError> Errors { get; private set; } = new();
    public DateTime ValidatedAt { get; private set; } = DateTime.UtcNow;

    private BusinessValidationResult(bool isValid)
    {
        IsValid = isValid;
    }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    public static BusinessValidationResult Success()
    {
        return new BusinessValidationResult(true);
    }

    /// <summary>
    /// Crea un resultado fallido con un error
    /// </summary>
    public static BusinessValidationResult Fail(string message, string? code = null, string? field = null)
    {
        var result = new BusinessValidationResult(false);
        result.Errors.Add(new BusinessValidationError(message, code, field));
        return result;
    }

    /// <summary>
    /// Crea un resultado fallido con múltiples errores
    /// </summary>
    public static BusinessValidationResult Fail(IEnumerable<BusinessValidationError> errors)
    {
        var result = new BusinessValidationResult(false);
        result.Errors.AddRange(errors);
        return result;
    }

    /// <summary>
    /// Combina múltiples resultados de validación
    /// </summary>
    public static BusinessValidationResult Combine(params BusinessValidationResult[] results)
    {
        var errors = results.SelectMany(r => r.Errors).ToList();
        if (errors.Any())
        {
            return Fail(errors);
        }
        return Success();
    }

    /// <summary>
    /// Agrega un error al resultado (lo convierte en inválido)
    /// </summary>
    public BusinessValidationResult AddError(string message, string? code = null, string? field = null)
    {
        IsValid = false;
        Errors.Add(new BusinessValidationError(message, code, field));
        return this;
    }
}

/// <summary>
/// Representa un error de validación de negocio
/// </summary>
public record BusinessValidationError(
    string Message,
    string? Code = null,
    string? Field = null,
    string? ValidatorId = null
);
