// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - Estrategia Temporal
// ═══════════════════════════════════════════════════════════════════════════════

using System.Reflection;

namespace ApiMotos.Application.Common.Validation.Strategies;

/// <summary>
/// Configuración para validación temporal
/// </summary>
public record TemporalValidatorConfig(
    string Campo,
    string Operador,      // "antes", "despues", "entre", "hace_menos_de", "hace_mas_de"
    object Valor,
    string? Unidad = null // "h", "d", "m", "y"
);

/// <summary>
/// Validador que evalúa condiciones temporales sobre campos de fecha
/// </summary>
public class TemporalValidator<TEntity> : IBusinessValidator<TEntity>
{
    public string ValidatorId { get; }
    public int Order { get; }

    private readonly TemporalValidatorConfig _config;
    private readonly string _mensajeError;

    public TemporalValidator(
        string validatorId,
        TemporalValidatorConfig config,
        string mensajeError,
        int order = 0)
    {
        ValidatorId = validatorId;
        _config = config;
        _mensajeError = mensajeError;
        Order = order;
    }

    public Task<BusinessValidationResult> ValidateAsync(object entity, CancellationToken ct = default)
    {
        return ValidateAsync((TEntity)entity, ct);
    }

    public Task<BusinessValidationResult> ValidateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null)
            return Task.FromResult(BusinessValidationResult.Fail("Entity is null"));

        var property = typeof(TEntity).GetProperty(
            _config.Campo,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property == null)
            return Task.FromResult(BusinessValidationResult.Success()); // Campo no existe, skip

        var value = property.GetValue(entity);
        if (value == null)
            return Task.FromResult(BusinessValidationResult.Success()); // Valor nulo, skip

        DateTime fieldDate;
        if (value is DateTime dt)
            fieldDate = dt;
        else if (value is DateTimeOffset dto)
            fieldDate = dto.DateTime;
        else if (value is string s && DateTime.TryParse(s, out var parsed))
            fieldDate = parsed;
        else
            return Task.FromResult(BusinessValidationResult.Success()); // No es fecha, skip

        var isValid = EvaluateCondition(fieldDate);

        return Task.FromResult(isValid
            ? BusinessValidationResult.Success()
            : BusinessValidationResult.Fail(_mensajeError, $"TEMPORAL_{_config.Operador.ToUpper()}", _config.Campo));
    }

    private bool EvaluateCondition(DateTime fieldDate)
    {
        var now = DateTime.Now;

        return _config.Operador.ToLower() switch
        {
            "antes" => fieldDate < ParseTargetDate(),
            "despues" => fieldDate > ParseTargetDate(),
            "hace_menos_de" => (now - fieldDate) < ParseTimeSpan(),
            "hace_mas_de" => (now - fieldDate) > ParseTimeSpan(),
            "entre" => EvaluateBetween(fieldDate),
            _ => true // Operador desconocido, skip
        };
    }

    private DateTime ParseTargetDate()
    {
        if (_config.Valor is DateTime dt)
            return dt;
        if (_config.Valor is string s && DateTime.TryParse(s, out var parsed))
            return parsed;
        return DateTime.MaxValue;
    }

    private TimeSpan ParseTimeSpan()
    {
        var value = Convert.ToDouble(_config.Valor);
        return _config.Unidad?.ToLower() switch
        {
            "h" => TimeSpan.FromHours(value),
            "d" => TimeSpan.FromDays(value),
            "m" => TimeSpan.FromDays(value * 30),
            "y" => TimeSpan.FromDays(value * 365),
            _ => TimeSpan.FromHours(value) // Default: horas
        };
    }

    private bool EvaluateBetween(DateTime fieldDate)
    {
        // Para "entre" se espera que Valor sea un objeto con Min y Max
        // Por ahora retornamos true
        return true;
    }
}
