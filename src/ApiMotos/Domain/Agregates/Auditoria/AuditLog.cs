using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Auditoria;

/// <summary>
/// Entidad de dominio para registro de auditoría.
/// Almacena todas las acciones realizadas en el sistema.
/// </summary>
public class AuditLog : BaseEntity<int>
{

    /// <summary>
    /// Momento exacto de la acción
    /// </summary>
    public DateTime Timestamp { get; private set; }

    /// <summary>
    /// ID del usuario que realizó la acción (null si anónimo)
    /// </summary>
    public int? UserId { get; private set; }

    /// <summary>
    /// Nombre del usuario para referencia rápida
    /// </summary>
    public string? UserName { get; private set; }

    /// <summary>
    /// Tipo de acción: Create, Update, Delete, Read, Login, Logout
    /// </summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo de entidad afectada: Usuario, Cliente, Producto, etc.
    /// </summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>
    /// ID de la entidad afectada (puede ser null en Create antes de persistir)
    /// </summary>
    public string? EntityId { get; private set; }

    /// <summary>
    /// Valores anteriores en formato JSON (para Update/Delete)
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// Valores nuevos en formato JSON (para Create/Update)
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// Dirección IP del cliente
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User-Agent del navegador/cliente
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Ruta del request HTTP
    /// </summary>
    public string? RequestPath { get; private set; }

    /// <summary>
    /// Duración de la operación en milisegundos
    /// </summary>
    public int? DurationMs { get; private set; }

    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Mensaje de error si la operación falló
    /// </summary>
    public string? ErrorMessage { get; private set; }

    // Constructor privado para EF
    private AuditLog() { }

    /// <summary>
    /// Crea un nuevo registro de auditoría
    /// </summary>
    public static AuditLog Crear(
        string action,
        string entityType,
        string? entityId,
        int? userId,
        string? userName,
        string? oldValues,
        string? newValues,
        string? ipAddress,
        string? userAgent,
        string? requestPath)
    {
        return new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            UserName = userName,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            RequestPath = requestPath,
            Success = true
        };
    }

    /// <summary>
    /// Marca la operación como completada exitosamente
    /// </summary>
    public void MarcarExitoso(int durationMs)
    {
        Success = true;
        DurationMs = durationMs;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marca la operación como fallida
    /// </summary>
    public void MarcarFallido(int durationMs, string errorMessage)
    {
        Success = false;
        DurationMs = durationMs;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Actualiza el EntityId (útil para Create donde el ID se genera después)
    /// </summary>
    public void SetEntityId(string entityId)
    {
        EntityId = entityId;
    }
}

/// <summary>
/// Tipos de acción para auditoría
/// </summary>
public static class AuditAction
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Read = "Read";
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string LoginFailed = "LoginFailed";
    public const string Export = "Export";
}
