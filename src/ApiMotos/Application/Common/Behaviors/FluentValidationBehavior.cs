using FluentValidation;
using MediatR;

namespace ApiMotos.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior para FluentValidation
/// Nivel 1 de validacion: Validacion sintactica de datos de entrada
/// Se ejecuta ANTES del Handler
/// </summary>
/// <typeparam name="TRequest">Tipo del request (Command/Query)</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta</typeparam>
public class FluentValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public FluentValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Si no hay validadores registrados, continuar
        if (!_validators.Any())
        {
            return await next();
        }

        // Crear contexto de validacion
        var context = new ValidationContext<TRequest>(request);

        // Ejecutar todos los validadores en paralelo
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Recoger todos los errores
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Si hay errores, lanzar excepcion de validacion
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        // Si no hay errores, continuar al handler
        return await next();
    }
}

/// <summary>
/// Excepcion personalizada para errores de validacion de entrada
/// Diferente de BusinessValidationException (errores de reglas de negocio)
/// </summary>
public class InputValidationException : Exception
{
    public IReadOnlyList<ValidationError> Errors { get; }

    public InputValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        : base("Error de validacion de entrada")
    {
        Errors = failures.Select(f => new ValidationError(
            f.PropertyName,
            f.ErrorMessage,
            f.ErrorCode
        )).ToList();
    }
}

/// <summary>
/// Error de validacion individual
/// </summary>
public record ValidationError(string PropertyName, string Message, string? Code);
