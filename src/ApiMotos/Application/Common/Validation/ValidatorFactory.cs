// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - Factory de Validadores
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text.Json;
using ApiMotos.Application.Common.Validation.Strategies;

namespace ApiMotos.Application.Common.Validation;

/// <summary>
/// Factory para crear validadores a partir de configuración JSON
/// </summary>
public static class ValidatorFactory
{
    /// <summary>
    /// Crea un validador a partir de la configuración JSON
    /// </summary>
    public static IBusinessValidator<TEntity> Create<TEntity>(
        string validatorId,
        string type,
        JsonElement config,
        string mensajeError,
        int order = 0,
        Func<object, CancellationToken, Task<DependenciaCheckResult>>? dependencyCheckFunc = null)
    {
        return type.ToLower() switch
        {
            "temporal" => CreateTemporalValidator<TEntity>(validatorId, config, mensajeError, order),
            "dependencia" => CreateDependenciaValidator<TEntity>(validatorId, config, mensajeError, order, dependencyCheckFunc),
            "atributo" => CreateAtributoValidator<TEntity>(validatorId, config, mensajeError, order),
            _ => new PassThroughValidator<TEntity>(validatorId)
        };
    }

    private static TemporalValidator<TEntity> CreateTemporalValidator<TEntity>(
        string validatorId,
        JsonElement config,
        string mensajeError,
        int order)
    {
        var campo = config.GetProperty("campo").GetString() ?? "";
        var operador = config.GetProperty("operador").GetString() ?? "";

        object valor = "";
        if (config.TryGetProperty("valor", out var valorProp))
        {
            valor = valorProp.ValueKind switch
            {
                JsonValueKind.Number => valorProp.GetDouble(),
                JsonValueKind.String => valorProp.GetString() ?? "",
                _ => ""
            };
        }

        string? unidad = null;
        if (config.TryGetProperty("unidad", out var unidadProp))
        {
            unidad = unidadProp.GetString();
        }

        return new TemporalValidator<TEntity>(
            validatorId,
            new TemporalValidatorConfig(campo, operador, valor, unidad),
            mensajeError,
            order);
    }

    private static DependenciaValidator<TEntity> CreateDependenciaValidator<TEntity>(
        string validatorId,
        JsonElement config,
        string mensajeError,
        int order,
        Func<object, CancellationToken, Task<DependenciaCheckResult>>? checkFunc)
    {
        var entidad = config.GetProperty("entidad").GetString() ?? "";
        var condicion = config.GetProperty("condicion").GetString() ?? "";

        string? campo = null;
        if (config.TryGetProperty("campo", out var campoProp))
            campo = campoProp.GetString();

        object? valor = null;
        if (config.TryGetProperty("valor", out var valorProp))
        {
            valor = valorProp.ValueKind switch
            {
                JsonValueKind.Number => valorProp.GetDouble(),
                JsonValueKind.String => valorProp.GetString(),
                _ => null
            };
        }

        string? relacionPor = null;
        if (config.TryGetProperty("relacionPor", out var relProp))
            relacionPor = relProp.GetString();

        return new DependenciaValidator<TEntity>(
            validatorId,
            new DependenciaValidatorConfig(entidad, condicion, campo, valor, relacionPor),
            mensajeError,
            checkFunc,
            order);
    }

    private static AtributoValidator<TEntity> CreateAtributoValidator<TEntity>(
        string validatorId,
        JsonElement config,
        string mensajeError,
        int order)
    {
        var campo = config.GetProperty("campo").GetString() ?? "";
        var condicion = config.GetProperty("condicion").GetString() ?? "";

        object? valor = null;
        if (config.TryGetProperty("valor", out var valorProp))
        {
            valor = valorProp.ValueKind switch
            {
                JsonValueKind.Number => valorProp.GetDouble(),
                JsonValueKind.String => valorProp.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }

        object? valorMax = null;
        if (config.TryGetProperty("valorMax", out var maxProp))
        {
            valorMax = maxProp.ValueKind switch
            {
                JsonValueKind.Number => maxProp.GetDouble(),
                JsonValueKind.String => maxProp.GetString(),
                _ => null
            };
        }

        return new AtributoValidator<TEntity>(
            validatorId,
            new AtributoValidatorConfig(campo, condicion, valor, valorMax),
            mensajeError,
            order);
    }
}

/// <summary>
/// Validador que siempre pasa (para tipos desconocidos)
/// </summary>
public class PassThroughValidator<TEntity> : IBusinessValidator<TEntity>
{
    public string ValidatorId { get; }
    public int Order => 999;

    public PassThroughValidator(string validatorId)
    {
        ValidatorId = validatorId;
    }

    public Task<BusinessValidationResult> ValidateAsync(object entity, CancellationToken ct = default)
        => Task.FromResult(BusinessValidationResult.Success());

    public Task<BusinessValidationResult> ValidateAsync(TEntity entity, CancellationToken ct = default)
        => Task.FromResult(BusinessValidationResult.Success());
}
