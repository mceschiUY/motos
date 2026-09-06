// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Atributos de Autorización
// Permite decorar Commands/Queries con [ZasAuthorize("Capability")]
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Shared.Infrastructure;

/// <summary>
/// Atributo para requerir una Capability específica para ejecutar un Command/Query.
/// Se puede aplicar múltiples veces para requerir varias capabilities.
/// </summary>
/// <example>
/// [ZasAuthorize("Lead.Crear")]
/// public record CrearLeadCommand(...) : ICommand&lt;Result&lt;int&gt;&gt;;
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class ZasAuthorizeAttribute : Attribute
{
    /// <summary>
    /// Capability requerida (ej: "Lead.Crear", "Cliente.Ver")
    /// </summary>
    public string Capability { get; }

    public ZasAuthorizeAttribute(string capability)
    {
        Capability = capability ?? throw new ArgumentNullException(nameof(capability));
    }
}

/// <summary>
/// Atributo para permitir acceso anónimo a un Command/Query.
/// Útil para endpoints públicos como Login, HealthCheck, etc.
/// </summary>
/// <example>
/// [ZasAllowAnonymous]
/// public record LoginQuery(...) : IQuery&lt;Result&lt;TokenDto&gt;&gt;;
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ZasAllowAnonymousAttribute : Attribute
{
}
