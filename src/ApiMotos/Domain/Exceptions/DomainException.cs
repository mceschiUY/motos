using ApiMotos.Domain.Specifications;

namespace ApiMotos.Domain.Exceptions;

/// <summary>
/// Excepcion base de dominio
/// Todas las excepciones de negocio heredan de esta
/// </summary>
public abstract class DomainException : Exception
{
    public DomainErrorCode ErrorCode { get; }
    public string? FieldName { get; }

    protected DomainException(DomainErrorCode errorCode, string message, string? fieldName = null)
        : base(message)
    {
        ErrorCode = errorCode;
        FieldName = fieldName;
    }

    protected DomainException(DomainErrorCode errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Crea excepcion desde un SpecificationResult
    /// </summary>
    public static DomainException FromSpecificationResult(SpecificationResult result)
    {
        if (result.IsValid)
            throw new InvalidOperationException("Cannot create exception from valid result");

        return new BusinessRuleViolationException(
            result.ErrorCode ?? DomainErrorCode.BusinessRuleViolation,
            result.ErrorMessage ?? "Violacion de regla de negocio",
            result.FieldName);
    }
}

/// <summary>
/// Violacion de regla de negocio generica
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message, string? fieldName = null)
        : base(DomainErrorCode.BusinessRuleViolation, message, fieldName)
    { }

    public BusinessRuleViolationException(DomainErrorCode errorCode, string message, string? fieldName = null)
        : base(errorCode, message, fieldName)
    { }
}

/// <summary>
/// Estado invalido de la entidad
/// </summary>
public class InvalidEntityStateException : DomainException
{
    public string EntityName { get; }
    public string CurrentState { get; }
    public string ExpectedState { get; }

    public InvalidEntityStateException(string entityName, string currentState, string expectedState)
        : base(DomainErrorCode.InvalidState,
            $"{entityName} debe estar en estado '{expectedState}' pero esta en '{currentState}'",
            "Estado")
    {
        EntityName = entityName;
        CurrentState = currentState;
        ExpectedState = expectedState;
    }
}

/// <summary>
/// Operacion fuera de tiempo permitido
/// </summary>
public class TemporalConstraintException : DomainException
{
    public TimeSpan AllowedPeriod { get; }
    public TimeSpan ElapsedTime { get; }

    public TemporalConstraintException(string message, TimeSpan allowedPeriod, TimeSpan elapsedTime, string? fieldName = null)
        : base(DomainErrorCode.TooLate, message, fieldName)
    {
        AllowedPeriod = allowedPeriod;
        ElapsedTime = elapsedTime;
    }
}

/// <summary>
/// Entidad tiene dependencias que impiden la operacion
/// </summary>
public class EntityHasDependenciesException : DomainException
{
    public string EntityName { get; }
    public string DependentEntityName { get; }
    public int DependentCount { get; }

    public EntityHasDependenciesException(string entityName, string dependentEntityName, int dependentCount = 0)
        : base(DomainErrorCode.HasDependencies,
            $"No se puede modificar/eliminar {entityName} porque tiene {(dependentCount > 0 ? dependentCount : "registros")} {dependentEntityName} asociados")
    {
        EntityName = entityName;
        DependentEntityName = dependentEntityName;
        DependentCount = dependentCount;
    }
}

/// <summary>
/// Entidad relacionada no encontrada
/// </summary>
public class RelatedEntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public RelatedEntityNotFoundException(string entityName, object entityId)
        : base(DomainErrorCode.RelatedEntityNotFound,
            $"No se encontro {entityName} con Id '{entityId}'")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

/// <summary>
/// Entidad relacionada inactiva
/// </summary>
public class RelatedEntityInactiveException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public RelatedEntityInactiveException(string entityName, object entityId)
        : base(DomainErrorCode.RelatedEntityInactive,
            $"{entityName} con Id '{entityId}' no esta activo")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

/// <summary>
/// Valor fuera de rango
/// </summary>
public class ValueOutOfRangeException : DomainException
{
    public string PropertyName { get; }
    public object? CurrentValue { get; }
    public object? MinValue { get; }
    public object? MaxValue { get; }

    public ValueOutOfRangeException(string propertyName, object? currentValue, object? minValue = null, object? maxValue = null)
        : base(DomainErrorCode.ValueOutOfRange,
            BuildMessage(propertyName, currentValue, minValue, maxValue),
            propertyName)
    {
        PropertyName = propertyName;
        CurrentValue = currentValue;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    private static string BuildMessage(string propertyName, object? currentValue, object? minValue, object? maxValue)
    {
        if (minValue != null && maxValue != null)
            return $"{propertyName} debe estar entre {minValue} y {maxValue} (valor actual: {currentValue})";
        if (minValue != null)
            return $"{propertyName} debe ser mayor o igual a {minValue} (valor actual: {currentValue})";
        if (maxValue != null)
            return $"{propertyName} debe ser menor o igual a {maxValue} (valor actual: {currentValue})";
        return $"{propertyName} tiene un valor fuera de rango: {currentValue}";
    }
}

/// <summary>
/// Stock/Balance insuficiente
/// </summary>
public class InsufficientResourceException : DomainException
{
    public string ResourceName { get; }
    public decimal Required { get; }
    public decimal Available { get; }

    public InsufficientResourceException(string resourceName, decimal required, decimal available)
        : base(DomainErrorCode.InsufficientStock,
            $"{resourceName} insuficiente: se requiere {required} pero solo hay {available} disponible",
            resourceName)
    {
        ResourceName = resourceName;
        Required = required;
        Available = available;
    }
}
