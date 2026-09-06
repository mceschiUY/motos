using ApiMotos.Domain.Exceptions;
using ApiMotos.Domain.Specifications;

namespace ApiMotos.Domain.Common;

/// <summary>
/// Result Pattern para operaciones de dominio
/// Alternativa a excepciones para flujo de control
/// </summary>
public readonly struct DomainResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public DomainErrorCode? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public string? FieldName { get; }

    private DomainResult(bool isSuccess, DomainErrorCode? errorCode, string? message, string? fieldName)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = message;
        FieldName = fieldName;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════════

    public static DomainResult Success() => new(true, null, null, null);

    public static DomainResult Failure(DomainErrorCode code, string message, string? fieldName = null)
        => new(false, code, message, fieldName);

    public static DomainResult Failure(SpecificationResult specResult)
        => new(false, specResult.ErrorCode, specResult.ErrorMessage, specResult.FieldName);

    public static DomainResult FromSpecification<T>(T entity, Specification<T> spec)
    {
        var result = spec.Evaluate(entity);
        return result.IsValid ? Success() : Failure(result);
    }

    public static DomainResult FromSpecifications<T>(T entity, params Specification<T>[] specs)
    {
        foreach (var spec in specs)
        {
            var result = spec.Evaluate(entity);
            if (!result.IsValid)
                return Failure(result);
        }
        return Success();
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // FLUENT METHODS
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Ejecuta accion si es exitoso
    /// </summary>
    public DomainResult OnSuccess(Action action)
    {
        if (IsSuccess) action();
        return this;
    }

    /// <summary>
    /// Ejecuta accion si fallo
    /// </summary>
    public DomainResult OnFailure(Action<DomainErrorCode?, string?> action)
    {
        if (IsFailure) action(ErrorCode, ErrorMessage);
        return this;
    }

    /// <summary>
    /// Lanza excepcion si fallo
    /// </summary>
    public DomainResult ThrowIfFailure()
    {
        if (IsFailure)
            throw new BusinessRuleViolationException(
                ErrorCode ?? DomainErrorCode.BusinessRuleViolation,
                ErrorMessage ?? "Error de validacion",
                FieldName);
        return this;
    }

    /// <summary>
    /// Combina con otro resultado (ambos deben ser exitosos)
    /// </summary>
    public DomainResult And(DomainResult other)
        => IsFailure ? this : other;

    /// <summary>
    /// Combina con otro resultado (al menos uno exitoso)
    /// </summary>
    public DomainResult Or(DomainResult other)
        => IsSuccess ? this : other;

    // ═══════════════════════════════════════════════════════════════════════════════
    // CONVERSION
    // ═══════════════════════════════════════════════════════════════════════════════

    public static implicit operator bool(DomainResult result) => result.IsSuccess;

    public override string ToString()
        => IsSuccess ? "Success" : $"Failure: [{ErrorCode}] {ErrorMessage}";
}

/// <summary>
/// Result Pattern con valor de retorno
/// </summary>
public readonly struct DomainResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public DomainErrorCode? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public string? FieldName { get; }

    private DomainResult(bool isSuccess, T? value, DomainErrorCode? errorCode, string? message, string? fieldName)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = message;
        FieldName = fieldName;
    }

    public static DomainResult<T> Success(T value) => new(true, value, null, null, null);

    public static DomainResult<T> Failure(DomainErrorCode code, string message, string? fieldName = null)
        => new(false, default, code, message, fieldName);

    public static DomainResult<T> Failure(SpecificationResult specResult)
        => new(false, default, specResult.ErrorCode, specResult.ErrorMessage, specResult.FieldName);

    /// <summary>
    /// Mapea el valor si es exitoso
    /// </summary>
    public DomainResult<TNew> Map<TNew>(Func<T, TNew> mapper)
        => IsSuccess
            ? DomainResult<TNew>.Success(mapper(Value!))
            : DomainResult<TNew>.Failure(ErrorCode!, ErrorMessage!, FieldName);

    /// <summary>
    /// Obtiene valor o lanza excepcion
    /// </summary>
    public T GetValueOrThrow()
    {
        if (IsFailure)
            throw new BusinessRuleViolationException(
                ErrorCode ?? DomainErrorCode.BusinessRuleViolation,
                ErrorMessage ?? "Error de validacion",
                FieldName);
        return Value!;
    }

    /// <summary>
    /// Obtiene valor o valor por defecto
    /// </summary>
    public T GetValueOrDefault(T defaultValue = default!)
        => IsSuccess ? Value! : defaultValue;

    public static implicit operator bool(DomainResult<T> result) => result.IsSuccess;
}
