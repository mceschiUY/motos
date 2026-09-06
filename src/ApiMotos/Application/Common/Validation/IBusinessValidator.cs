// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - Interfaces de Validadores
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Application.Common.Validation;

/// <summary>
/// Interfaz base para validadores de negocio
/// </summary>
public interface IBusinessValidator
{
    string ValidatorId { get; }
    int Order { get; }
    Task<BusinessValidationResult> ValidateAsync(object entity, CancellationToken ct = default);
}

/// <summary>
/// Interfaz genérica para validadores tipados
/// </summary>
public interface IBusinessValidator<TEntity> : IBusinessValidator
{
    Task<BusinessValidationResult> ValidateAsync(TEntity entity, CancellationToken ct = default);
}

/// <summary>
/// Interfaz para entidades que soportan validación de capabilities
/// </summary>
public interface IValidatableEntity
{
    /// <summary>
    /// Valida si se puede ejecutar una acción específica
    /// </summary>
    Task<BusinessValidationResult> PuedeEjecutarAsync(string actionKey, CancellationToken ct = default);
}

/// <summary>
/// Atributo para marcar Commands/Queries que requieren validación de agregado
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RequiresAggregateValidationAttribute : Attribute
{
    public string ActionKey { get; }
    public Type EntityType { get; }

    public RequiresAggregateValidationAttribute(Type entityType, string actionKey)
    {
        EntityType = entityType;
        ActionKey = actionKey;
    }
}

/// <summary>
/// Atributo para excluir un Command/Query del pipeline de validación
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SkipValidationAttribute : Attribute
{
}
