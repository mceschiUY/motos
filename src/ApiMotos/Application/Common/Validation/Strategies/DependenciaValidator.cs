// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - Estrategia de Dependencia
// ═══════════════════════════════════════════════════════════════════════════════

using System.Reflection;

namespace ApiMotos.Application.Common.Validation.Strategies;

/// <summary>
/// Configuración para validación de dependencias entre entidades
/// </summary>
public record DependenciaValidatorConfig(
    string Entidad,           // Ej: "Pago", "Cotizacion"
    string Condicion,         // "existe", "no_existe", "estado_es", "cantidad_mayor", etc.
    string? Campo = null,     // Campo a evaluar
    object? Valor = null,     // Valor esperado
    string? RelacionPor = null // FK que relaciona
);

/// <summary>
/// Validador que evalúa dependencias con otras entidades
/// Requiere inyección del servicio de consulta correspondiente
/// </summary>
public class DependenciaValidator<TEntity> : IBusinessValidator<TEntity>
{
    public string ValidatorId { get; }
    public int Order { get; }

    private readonly DependenciaValidatorConfig _config;
    private readonly string _mensajeError;
    private readonly Func<object, CancellationToken, Task<DependenciaCheckResult>>? _checkFunc;

    public DependenciaValidator(
        string validatorId,
        DependenciaValidatorConfig config,
        string mensajeError,
        Func<object, CancellationToken, Task<DependenciaCheckResult>>? checkFunc = null,
        int order = 0)
    {
        ValidatorId = validatorId;
        _config = config;
        _mensajeError = mensajeError;
        _checkFunc = checkFunc;
        Order = order;
    }

    public Task<BusinessValidationResult> ValidateAsync(object entity, CancellationToken ct = default)
    {
        return ValidateAsync((TEntity)entity, ct);
    }

    public async Task<BusinessValidationResult> ValidateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
            return BusinessValidationResult.Fail("Entity is null");

        // Si no hay función de verificación, usar reflexión para obtener el ID
        if (_checkFunc == null)
        {
            // Sin función de check, retornamos success (se configurará en runtime)
            return BusinessValidationResult.Success();
        }

        var checkResult = await _checkFunc(entity!, ct);
        var isValid = EvaluateCondition(checkResult);

        return isValid
            ? BusinessValidationResult.Success()
            : BusinessValidationResult.Fail(_mensajeError, $"DEP_{_config.Condicion.ToUpper()}", _config.Entidad);
    }

    private bool EvaluateCondition(DependenciaCheckResult result)
    {
        return _config.Condicion.ToLower() switch
        {
            "existe" => result.Exists,
            "no_existe" => !result.Exists,
            "estado_es" => result.FieldValue?.ToString() == _config.Valor?.ToString(),
            "cantidad_mayor" => result.Count > Convert.ToInt32(_config.Valor ?? 0),
            "cantidad_menor" => result.Count < Convert.ToInt32(_config.Valor ?? 0),
            "cantidad_igual" => result.Count == Convert.ToInt32(_config.Valor ?? 0),
            _ => true
        };
    }
}

/// <summary>
/// Resultado de verificación de dependencia
/// </summary>
public record DependenciaCheckResult(
    bool Exists,
    int Count = 0,
    object? FieldValue = null
);
