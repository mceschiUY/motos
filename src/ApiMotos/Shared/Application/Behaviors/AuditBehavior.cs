using System.Diagnostics;
using System.Text.Json;
using MediatR;
using ApiMotos.Domain.Agregates.Auditoria;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Shared.Application.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior para auditoría automática.
/// Registra todas las acciones de Commands que implementen IAuditableRequest.
/// Se ejecuta DESPUÉS de validación y ANTES del handler.
/// </summary>
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

    public AuditBehavior(
        IServiceScopeFactory scopeFactory,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditBehavior<TRequest, TResponse>> logger)
    {
        _scopeFactory = scopeFactory;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Solo auditar si el request implementa IAuditableRequest
        if (request is not IAuditableRequest auditableRequest)
        {
            return await next();
        }

        var requestType = typeof(TRequest);
        var requestName = requestType.Name;
        var stopwatch = Stopwatch.StartNew();

        // Determinar tipo de acción basado en el nombre del Command
        var action = DeterminarAccion(requestName);

        // Capturar información del contexto HTTP
        var httpContext = _httpContextAccessor.HttpContext;
        var ipAddress = ObtenerIpAddress(httpContext);
        var userAgent = httpContext?.Request.Headers["User-Agent"].FirstOrDefault();
        var requestPath = httpContext?.Request.Path.Value;

        // Obtener valores para auditoría
        string? oldValues = null;
        string? newValues = null;

        if (request is IAuditableCommand auditableCommand)
        {
            try
            {
                var oldObj = auditableCommand.GetOldValues();
                var newObj = auditableCommand.GetNewValues();

                oldValues = oldObj != null ? SerializarValores(oldObj) : null;
                newValues = newObj != null ? SerializarValores(newObj) : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Audit] Error serializando valores para {Request}", requestName);
            }
        }

        // Crear registro de auditoría
        var auditLog = AuditLog.Crear(
            action: action,
            entityType: auditableRequest.GetEntityType(),
            entityId: auditableRequest.GetEntityId(),
            userId: _currentUser.IsAuthenticated ? _currentUser.UserId : null,
            userName: _currentUser.IsAuthenticated ? _currentUser.UserName : null,
            oldValues: oldValues,
            newValues: newValues,
            ipAddress: ipAddress,
            userAgent: userAgent?.Length > 500 ? userAgent[..500] : userAgent,
            requestPath: requestPath?.Length > 500 ? requestPath[..500] : requestPath
        );

        try
        {
            // Ejecutar el handler real
            var response = await next();

            stopwatch.Stop();
            auditLog.MarcarExitoso((int)stopwatch.ElapsedMilliseconds);

            // Si el response contiene el ID de la entidad creada, actualizarlo
            if (action == AuditAction.Create && auditLog.EntityId == null)
            {
                var entityId = ExtraerEntityId(response);
                if (entityId != null)
                {
                    auditLog.SetEntityId(entityId);
                }
            }

            // Persistir auditoría de forma asíncrona (fire-and-forget para no bloquear)
            _ = PersistirAuditoriaAsync(auditLog);

            _logger.LogDebug(
                "[Audit] {Action} {EntityType} {EntityId} por {User} - {Duration}ms",
                action, auditableRequest.GetEntityType(), auditableRequest.GetEntityId(),
                _currentUser.UserName ?? "Anónimo", stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            auditLog.MarcarFallido((int)stopwatch.ElapsedMilliseconds, ex.Message);

            // Persistir auditoría de error
            _ = PersistirAuditoriaAsync(auditLog);

            _logger.LogWarning(
                "[Audit] FALLIDO {Action} {EntityType} por {User} - Error: {Error}",
                action, auditableRequest.GetEntityType(),
                _currentUser.UserName ?? "Anónimo", ex.Message);

            throw;
        }
    }

    private string DeterminarAccion(string requestName)
    {
        var nameLower = requestName.ToLower();

        if (nameLower.Contains("crear") || nameLower.Contains("create") || nameLower.Contains("add"))
            return AuditAction.Create;

        if (nameLower.Contains("modificar") || nameLower.Contains("update") || nameLower.Contains("actualizar") || nameLower.Contains("edit"))
            return AuditAction.Update;

        if (nameLower.Contains("eliminar") || nameLower.Contains("delete") || nameLower.Contains("remove") || nameLower.Contains("borrar"))
            return AuditAction.Delete;

        if (nameLower.Contains("login"))
            return AuditAction.Login;

        if (nameLower.Contains("logout"))
            return AuditAction.Logout;

        if (nameLower.Contains("export"))
            return AuditAction.Export;

        // Por defecto, si es Query es Read, si es Command es Update
        return nameLower.Contains("query") || nameLower.Contains("get") || nameLower.Contains("obtener")
            ? AuditAction.Read
            : AuditAction.Update;
    }

    private string? ObtenerIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null) return null;

        // Intentar obtener IP real detrás de proxy
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string SerializarValores(object obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
        catch
        {
            return obj.ToString() ?? "";
        }
    }

    private string? ExtraerEntityId(TResponse response)
    {
        if (response == null) return null;

        try
        {
            // Intentar extraer ID de diferentes tipos de response
            var responseType = response.GetType();

            // FluentResults.Result<int>
            if (responseType.IsGenericType)
            {
                var valueProp = responseType.GetProperty("Value");
                if (valueProp != null)
                {
                    var value = valueProp.GetValue(response);
                    if (value != null)
                    {
                        return value.ToString();
                    }
                }
            }

            // Si es directamente un int
            if (response is int intValue)
            {
                return intValue.ToString();
            }

            // Si tiene propiedad Id
            var idProp = responseType.GetProperty("Id");
            if (idProp != null)
            {
                var idValue = idProp.GetValue(response);
                return idValue?.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[Audit] No se pudo extraer EntityId del response");
        }

        return null;
    }

    private async Task PersistirAuditoriaAsync(AuditLog auditLog)
    {
        try
        {
            // Scope propio: este método corre fire-and-forget y el scope del request
            // (con su AuditLogContext) ya puede estar dispuesto cuando llega acá.
            using var scope = _scopeFactory.CreateScope();
            var auditRepo = scope.ServiceProvider.GetRequiredService<IAuditLogRepositorio>();
            await auditRepo.CrearAsync(auditLog);
        }
        catch (Exception ex)
        {
            // Solo loguear, no fallar la operación principal
            _logger.LogError(ex, "[Audit] Error persistiendo auditoría para {EntityType}", auditLog.EntityType);
        }
    }
}
