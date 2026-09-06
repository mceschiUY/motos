using System.Linq.Expressions;

namespace ApiMotos.Domain.Specifications;

/// <summary>
/// Clase base abstracta para Specifications
/// Implementa Composite Pattern para combinar especificaciones
/// </summary>
public abstract class Specification<T> : ISpecificationWithResult<T>
{
    public abstract DomainErrorCode ErrorCode { get; }
    public abstract string ErrorMessage { get; }
    public virtual string? FieldName => null;

    /// <summary>
    /// Implementacion de la logica de validacion
    /// </summary>
    public abstract bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Evalua y retorna resultado completo
    /// </summary>
    public SpecificationResult Evaluate(T entity)
    {
        return IsSatisfiedBy(entity)
            ? SpecificationResult.Valid()
            : SpecificationResult.Invalid(ErrorCode, ErrorMessage, FieldName);
    }

    /// <summary>
    /// Combina con AND
    /// </summary>
    public Specification<T> And(Specification<T> other) => new AndSpecification<T>(this, other);

    /// <summary>
    /// Combina con OR
    /// </summary>
    public Specification<T> Or(Specification<T> other) => new OrSpecification<T>(this, other);

    /// <summary>
    /// Niega la especificacion
    /// </summary>
    public Specification<T> Not() => new NotSpecification<T>(this);

    /// <summary>
    /// Operador && para combinar specs
    /// </summary>
    public static Specification<T> operator &(Specification<T> left, Specification<T> right)
        => left.And(right);

    /// <summary>
    /// Operador || para combinar specs
    /// </summary>
    public static Specification<T> operator |(Specification<T> left, Specification<T> right)
        => left.Or(right);

    /// <summary>
    /// Operador ! para negar
    /// </summary>
    public static Specification<T> operator !(Specification<T> spec)
        => spec.Not();
}

/// <summary>
/// Specification AND (ambas deben cumplirse)
/// </summary>
internal sealed class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override DomainErrorCode ErrorCode => _left.ErrorCode;
    public override string ErrorMessage => _left.ErrorMessage;
    public override string? FieldName => _left.FieldName;

    public override bool IsSatisfiedBy(T entity)
    {
        // Primero evalua left, si falla retorna false
        if (!_left.IsSatisfiedBy(entity))
            return false;

        // Si left pasa, evalua right
        return _right.IsSatisfiedBy(entity);
    }

    // Sobrescribe Evaluate para dar el error correcto
    public new SpecificationResult Evaluate(T entity)
    {
        var leftResult = _left.Evaluate(entity);
        if (!leftResult.IsValid)
            return leftResult;

        return _right.Evaluate(entity);
    }
}

/// <summary>
/// Specification OR (al menos una debe cumplirse)
/// </summary>
internal sealed class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override DomainErrorCode ErrorCode => _left.ErrorCode;
    public override string ErrorMessage => $"{_left.ErrorMessage} O {_right.ErrorMessage}";

    public override bool IsSatisfiedBy(T entity)
        => _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);
}

/// <summary>
/// Specification NOT (niega el resultado)
/// </summary>
internal sealed class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _inner;

    public NotSpecification(Specification<T> inner)
    {
        _inner = inner;
    }

    public override DomainErrorCode ErrorCode => _inner.ErrorCode;
    public override string ErrorMessage => $"NO debe cumplir: {_inner.ErrorMessage}";

    public override bool IsSatisfiedBy(T entity) => !_inner.IsSatisfiedBy(entity);
}

/// <summary>
/// Extension methods para evaluar multiples specifications
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Evalua todas las specifications y retorna el primer error
    /// </summary>
    public static SpecificationResult EvaluateAll<T>(this T entity, params Specification<T>[] specs)
    {
        foreach (var spec in specs)
        {
            var result = spec.Evaluate(entity);
            if (!result.IsValid)
                return result;
        }
        return SpecificationResult.Valid();
    }

    /// <summary>
    /// Evalua todas y retorna todos los errores
    /// </summary>
    public static IReadOnlyList<SpecificationResult> EvaluateAllErrors<T>(this T entity, params Specification<T>[] specs)
    {
        var errors = new List<SpecificationResult>();
        foreach (var spec in specs)
        {
            var result = spec.Evaluate(entity);
            if (!result.IsValid)
                errors.Add(result);
        }
        return errors;
    }

    /// <summary>
    /// Retorna true si cumple todas las specs
    /// </summary>
    public static bool SatisfiesAll<T>(this T entity, params Specification<T>[] specs)
        => specs.All(s => s.IsSatisfiedBy(entity));

    /// <summary>
    /// Retorna true si cumple al menos una spec
    /// </summary>
    public static bool SatisfiesAny<T>(this T entity, params Specification<T>[] specs)
        => specs.Any(s => s.IsSatisfiedBy(entity));
}
