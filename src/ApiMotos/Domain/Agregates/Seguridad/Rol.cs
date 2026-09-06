// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Entidad Rol
// Agrupa múltiples Capabilities bajo un nombre lógico
// Ejemplos: "Vendedor", "Supervisor", "Administrador"
// ═══════════════════════════════════════════════════════════════════════════════

using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Seguridad;

/// <summary>
/// Rol: Agrupa Capabilities relacionadas.
/// Un perfil puede tener múltiples roles.
/// </summary>
public class Rol : BaseEntity<int>
{
    /// <summary>
    /// Nombre único del rol (ej: "Vendedor", "Administrador")
    /// </summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>
    /// Descripción del rol y sus responsabilidades
    /// </summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Indica si el rol está activo
    /// </summary>
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// Fecha de creación del rol
    /// </summary>
    public DateTime FechaCreacion { get; private set; } = DateTime.Now;

    /// <summary>
    /// Capabilities asignadas a este rol (navegación)
    /// </summary>
    public virtual ICollection<RolCapability> RolCapabilities { get; private set; } = new List<RolCapability>();

    /// <summary>
    /// Perfiles que tienen este rol (navegación)
    /// </summary>
    public virtual ICollection<PerfilRol> PerfilRoles { get; private set; } = new List<PerfilRol>();

    // Constructor para EF
    protected Rol() { }

    public Rol(string nombre, string descripcion = "")
    {
        SetNombre(nombre);
        Descripcion = descripcion;
    }

    public void SetNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del rol no puede estar vacío", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarDescripcion(string descripcion)
    {
        Descripcion = descripcion ?? string.Empty;
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;

    /// <summary>
    /// Agrega una capability al rol
    /// </summary>
    public void AgregarCapability(Capability capability)
    {
        if (capability == null)
            throw new ArgumentNullException(nameof(capability));

        if (RolCapabilities.Any(rc => rc.CapabilityId == capability.Id))
            return; // Ya existe

        RolCapabilities.Add(new RolCapability
        {
            Rol = this,
            RolId = this.Id,
            Capability = capability,
            CapabilityId = capability.Id,
            FechaAsignacion = DateTime.Now
        });
    }

    /// <summary>
    /// Remueve una capability del rol
    /// </summary>
    public void RemoverCapability(int capabilityId)
    {
        var rolCapability = RolCapabilities.FirstOrDefault(rc => rc.CapabilityId == capabilityId);
        if (rolCapability != null)
        {
            RolCapabilities.Remove(rolCapability);
        }
    }

    /// <summary>
    /// Obtiene los nombres de todas las capabilities del rol
    /// </summary>
    public IEnumerable<string> ObtenerCapabilities()
    {
        return RolCapabilities
            .Where(rc => rc.Capability.Activo)
            .Select(rc => rc.Capability.Nombre);
    }
}
