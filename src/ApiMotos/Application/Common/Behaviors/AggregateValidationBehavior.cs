// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE VALIDACIÓN UNIVERSAL - MediatR Pipeline Behavior
// ═══════════════════════════════════════════════════════════════════════════════

using System.Reflection;
using MediatR;
using ApiMotos.Application.Common.Validation;

namespace ApiMotos.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que ejecuta validación de agregado antes de cada Command/Query
/// Si el Command tiene [SkipValidation], se salta la validación
/// Si el Command tiene [RequiresAggregateValidation], busca el método PuedeEjecutar en la entidad
/// </summary>
public class AggregateValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IServiceProvider _serviceProvider;

    public AggregateValidationBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);

        // Verificar si tiene [SkipValidation]
        if (requestType.GetCustomAttribute<SkipValidationAttribute>() != null)
        {
            return await next();
        }

        // Verificar si tiene [RequiresAggregateValidation]
        var validationAttr = requestType.GetCustomAttribute<RequiresAggregateValidationAttribute>();
        if (validationAttr != null)
        {
            await ValidateWithAttribute(request, validationAttr, cancellationToken);
        }

        // Intentar validación por convención
        await ValidateByConvention(request, cancellationToken);

        return await next();
    }

    private async Task ValidateWithAttribute(
        TRequest request,
        RequiresAggregateValidationAttribute attr,
        CancellationToken ct)
    {
        // Buscar el repositorio para obtener la entidad
        var repositoryType = typeof(IRepository<,>).MakeGenericType(attr.EntityType, typeof(int));
        var repository = _serviceProvider.GetService(repositoryType);

        if (repository == null) return;

        // Obtener el ID del request
        var idProperty = typeof(TRequest).GetProperty("Id") ??
                        typeof(TRequest).GetProperty($"{attr.EntityType.Name}Id");

        if (idProperty == null) return;

        var id = idProperty.GetValue(request);
        if (id == null) return;

        // Obtener la entidad
        var findMethod = repositoryType.GetMethod("FindAsync");
        if (findMethod == null) return;

        var entityTask = findMethod.Invoke(repository, new[] { id }) as Task;
        if (entityTask == null) return;

        await entityTask;
        var resultProperty = entityTask.GetType().GetProperty("Result");
        var entity = resultProperty?.GetValue(entityTask);

        if (entity == null) return;

        // Buscar método PuedeEjecutar{ActionKey}
        var methodName = $"PuedeEjecutar{ToPascalCase(attr.ActionKey)}";
        var method = attr.EntityType.GetMethod(methodName);

        if (method == null)
        {
            // Buscar método genérico PuedeEjecutarAsync
            method = attr.EntityType.GetMethod("PuedeEjecutarAsync");
        }

        if (method == null) return;

        // Invocar validación
        var result = await InvokeValidationMethod(entity, method, attr.ActionKey, ct);

        if (!result.IsValid)
        {
            throw new BusinessValidationException(result, attr.EntityType.Name, attr.ActionKey);
        }
    }

    private async Task ValidateByConvention(TRequest request, CancellationToken ct)
    {
        // Detectar por nombre del Command: Crear{Entity}Command, Modificar{Entity}Command, etc.
        var requestName = typeof(TRequest).Name;

        var actions = new[] { "Crear", "Modificar", "Eliminar", "Create", "Update", "Delete" };
        string? detectedAction = null;
        string? entityName = null;

        foreach (var action in actions)
        {
            if (requestName.StartsWith(action))
            {
                detectedAction = action;
                entityName = requestName
                    .Substring(action.Length)
                    .Replace("Command", "")
                    .Replace("Query", "");
                break;
            }
        }

        if (detectedAction == null || string.IsNullOrEmpty(entityName)) return;

        // Buscar si el request implementa una interfaz que indique la entidad
        var entityInterface = typeof(TRequest).GetInterfaces()
            .FirstOrDefault(i => i.Name.Contains("Entity") || i.Name.Contains("Aggregate"));

        // Por ahora, solo logueamos que detectamos el patrón
        // La validación real se hace con el atributo [RequiresAggregateValidation]
    }

    private async Task<BusinessValidationResult> InvokeValidationMethod(
        object entity,
        MethodInfo method,
        string actionKey,
        CancellationToken ct)
    {
        object? result;

        if (method.GetParameters().Length == 0)
        {
            result = method.Invoke(entity, null);
        }
        else if (method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType == typeof(string))
        {
            result = method.Invoke(entity, new object[] { actionKey });
        }
        else if (method.GetParameters().Length == 2)
        {
            result = method.Invoke(entity, new object[] { actionKey, ct });
        }
        else
        {
            return BusinessValidationResult.Success();
        }

        if (result is Task<BusinessValidationResult> taskResult)
        {
            return await taskResult;
        }

        if (result is BusinessValidationResult syncResult)
        {
            return syncResult;
        }

        return BusinessValidationResult.Success();
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1);
    }
}

/// <summary>
/// Interfaz de repositorio genérico (para reflection)
/// </summary>
public interface IRepository<TEntity, TId>
{
    Task<TEntity?> FindAsync(TId id);
}
