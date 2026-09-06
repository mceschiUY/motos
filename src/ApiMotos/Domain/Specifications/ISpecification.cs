namespace ApiMotos.Domain.Specifications;

/// <summary>
/// Specification Pattern - Base interface
/// Permite encapsular reglas de negocio en clases reutilizables
/// Principio: Single Responsibility + Open/Closed
/// </summary>
/// <typeparam name="T">Tipo de entidad a validar</typeparam>
public interface ISpecification<in T>
{
    /// <summary>
    /// Evalua si la entidad cumple con la especificacion
    /// </summary>
    bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Codigo de error tipado si no se cumple
    /// </summary>
    DomainErrorCode ErrorCode { get; }

    /// <summary>
    /// Mensaje de error descriptivo
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// Campo relacionado con el error (opcional)
    /// </summary>
    string? FieldName { get; }
}

/// <summary>
/// Specification con resultado detallado
/// </summary>
public interface ISpecificationWithResult<in T> : ISpecification<T>
{
    /// <summary>
    /// Evalua y retorna resultado tipado
    /// </summary>
    SpecificationResult Evaluate(T entity);
}

/// <summary>
/// Resultado de evaluacion de Specification
/// </summary>
public readonly struct SpecificationResult
{
    public bool IsValid { get; }
    public DomainErrorCode? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public string? FieldName { get; }

    private SpecificationResult(bool isValid, DomainErrorCode? errorCode, string? message, string? field)
    {
        IsValid = isValid;
        ErrorCode = errorCode;
        ErrorMessage = message;
        FieldName = field;
    }

    public static SpecificationResult Valid() => new(true, null, null, null);

    public static SpecificationResult Invalid(DomainErrorCode code, string message, string? field = null)
        => new(false, code, message, field);
}
