// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - Estrategia de Atributo
// ═══════════════════════════════════════════════════════════════════════════════

using System.Reflection;

namespace ApiMotos.Application.Common.Validation.Strategies;

/// <summary>
/// Configuración para validación de atributos
/// </summary>
public record AtributoValidatorConfig(
    string Campo,
    string Condicion,         // "es", "no_es", "contiene", "mayor_que", "menor_que", "entre", "vacio", "no_vacio"
    object? Valor = null,
    object? ValorMax = null   // Para "entre"
);

/// <summary>
/// Validador que evalúa condiciones sobre atributos de la entidad
/// </summary>
public class AtributoValidator<TEntity> : IBusinessValidator<TEntity>
{
    public string ValidatorId { get; }
    public int Order { get; }

    private readonly AtributoValidatorConfig _config;
    private readonly string _mensajeError;

    public AtributoValidator(
        string validatorId,
        AtributoValidatorConfig config,
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
        var isValid = EvaluateCondition(value);

        return Task.FromResult(isValid
            ? BusinessValidationResult.Success()
            : BusinessValidationResult.Fail(_mensajeError, $"ATTR_{_config.Condicion.ToUpper()}", _config.Campo));
    }

    private bool EvaluateCondition(object? value)
    {
        return _config.Condicion.ToLower() switch
        {
            "es" => AreEqual(value, _config.Valor),
            "no_es" => !AreEqual(value, _config.Valor),
            "contiene" => value?.ToString()?.Contains(_config.Valor?.ToString() ?? "") ?? false,
            "no_contiene" => !(value?.ToString()?.Contains(_config.Valor?.ToString() ?? "") ?? true),
            "mayor_que" => CompareNumeric(value, _config.Valor) > 0,
            "menor_que" => CompareNumeric(value, _config.Valor) < 0,
            "entre" => IsBetween(value),
            "vacio" => IsEmpty(value),
            "no_vacio" => !IsEmpty(value),
            _ => true
        };
    }

    private static bool AreEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.ToString()?.Equals(b.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private static int CompareNumeric(object? value, object? target)
    {
        try
        {
            var v = Convert.ToDouble(value);
            var t = Convert.ToDouble(target);
            return v.CompareTo(t);
        }
        catch
        {
            return 0;
        }
    }

    private bool IsBetween(object? value)
    {
        try
        {
            var v = Convert.ToDouble(value);
            var min = Convert.ToDouble(_config.Valor);
            var max = Convert.ToDouble(_config.ValorMax);
            return v >= min && v <= max;
        }
        catch
        {
            return true;
        }
    }

    private static bool IsEmpty(object? value)
    {
        if (value == null) return true;
        if (value is string s) return string.IsNullOrWhiteSpace(s);
        return false;
    }
}
