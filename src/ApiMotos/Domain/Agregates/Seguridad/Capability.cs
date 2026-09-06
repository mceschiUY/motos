// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Entidad Capability
// Representa una acción atómica que un usuario puede realizar
// Ejemplos: "Lead.Crear", "Lead.Ver", "Cliente.Modificar"
// ═══════════════════════════════════════════════════════════════════════════════

using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Seguridad;

/// <summary>
/// Capability: Acción atómica del sistema.
/// Naming convention: "{Entidad}.{Accion}" (ej: "Lead.Crear", "Cliente.Ver")
/// </summary>
public class Capability : BaseEntity<int>
{
    /// <summary>
    /// Nombre único de la capability (ej: "Lead.Crear")
    /// </summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>
    /// Descripción legible de la capability
    /// </summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Módulo al que pertenece (ej: "Lead", "Cliente", "Pedido")
    /// </summary>
    public string Modulo { get; private set; } = string.Empty;

    /// <summary>
    /// Indica si la capability está activa
    /// </summary>
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// Roles que tienen esta capability (navegación)
    /// </summary>
    public virtual ICollection<RolCapability> RolCapabilities { get; private set; } = new List<RolCapability>();

    // Constructor para EF
    protected Capability() { }

    public Capability(string nombre, string descripcion, string modulo)
    {
        SetNombre(nombre);
        Descripcion = descripcion;
        Modulo = modulo;
    }

    public void SetNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la capability no puede estar vacío", nameof(nombre));

        // Validar formato: Modulo.Accion
        if (!nombre.Contains('.'))
            throw new ArgumentException("El nombre debe tener formato 'Modulo.Accion'", nameof(nombre));

        Nombre = nombre;
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;

    public void ActualizarDescripcion(string descripcion)
    {
        Descripcion = descripcion ?? string.Empty;
    }
}

/// <summary>
/// Tabla intermedia Rol-Capability (N:M)
/// </summary>
public class RolCapability
{
    public int RolId { get; set; }
    public virtual Rol Rol { get; set; } = null!;

    public int CapabilityId { get; set; }
    public virtual Capability Capability { get; set; } = null!;

    /// <summary>
    /// Fecha en que se asignó la capability al rol
    /// </summary>
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
}
