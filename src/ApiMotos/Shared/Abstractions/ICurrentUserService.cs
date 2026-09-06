// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Interface de Usuario Actual
// Abstracción para obtener información del usuario autenticado
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Shared.Abstractions;

/// <summary>
/// Servicio para obtener información del usuario actual (autenticado).
/// Lee claims del token JWT en cada request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// ID del usuario actual (null si no autenticado)
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Nombre de usuario actual
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// ID del perfil del usuario
    /// </summary>
    int? PerfilId { get; }

    /// <summary>
    /// Indica si el usuario está autenticado
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Lista de capabilities del usuario (cacheada por request)
    /// </summary>
    IReadOnlyList<string> Capabilities { get; }

    /// <summary>
    /// Verifica si el usuario tiene una capability específica
    /// </summary>
    /// <param name="capability">Nombre de la capability (ej: "Lead.Crear")</param>
    bool HasCapability(string capability);

    /// <summary>
    /// Verifica si el usuario tiene al menos una de las capabilities
    /// </summary>
    bool HasAnyCapability(params string[] capabilities);

    /// <summary>
    /// Verifica si el usuario tiene todas las capabilities
    /// </summary>
    bool HasAllCapabilities(params string[] capabilities);
}
