// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Excepciones
// Excepciones específicas para errores de autenticación y autorización
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Shared.Exceptions;

/// <summary>
/// Códigos de error de seguridad con su código HTTP correspondiente
/// </summary>
public enum SecurityErrorCode
{
    /// <summary>Usuario no autenticado - HTTP 401</summary>
    NotAuthenticated = 401,

    /// <summary>Usuario sin permisos - HTTP 403</summary>
    AccessDenied = 403,

    /// <summary>Token expirado - HTTP 401</summary>
    TokenExpired = 401,

    /// <summary>Token inválido - HTTP 401</summary>
    InvalidToken = 401
}

/// <summary>
/// Excepción de seguridad para errores de autenticación/autorización.
/// Capturada por BusinessValidationMiddleware y mapeada a HTTP 401/403.
/// </summary>
public class SecurityException : Exception
{
    /// <summary>
    /// ID del usuario que intentó la acción (si estaba autenticado)
    /// </summary>
    public int? UserId { get; init; }

    /// <summary>
    /// Capability requerida que faltaba
    /// </summary>
    public string? RequiredCapability { get; init; }

    /// <summary>
    /// Código de error de seguridad
    /// </summary>
    public SecurityErrorCode ErrorCode { get; init; }

    public SecurityException(string message, SecurityErrorCode errorCode = SecurityErrorCode.AccessDenied)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public SecurityException(string message, string requiredCapability)
        : base(message)
    {
        RequiredCapability = requiredCapability;
        ErrorCode = SecurityErrorCode.AccessDenied;
    }

    public SecurityException(string message, int userId, string requiredCapability)
        : base(message)
    {
        UserId = userId;
        RequiredCapability = requiredCapability;
        ErrorCode = SecurityErrorCode.AccessDenied;
    }

    /// <summary>
    /// Crea excepción de usuario no autenticado
    /// </summary>
    public static SecurityException NotAuthenticated(string? capability = null) =>
        new($"Usuario no autenticado", SecurityErrorCode.NotAuthenticated)
        {
            RequiredCapability = capability
        };

    /// <summary>
    /// Crea excepción de acceso denegado
    /// </summary>
    public static SecurityException Forbidden(string capability, int? userId = null)
    {
        var message = $"No tiene permiso para ejecutar esta acción. Capability requerida: {capability}";
        return userId.HasValue
            ? new SecurityException(message, userId.Value, capability)
            : new SecurityException(message, capability);
    }
}
