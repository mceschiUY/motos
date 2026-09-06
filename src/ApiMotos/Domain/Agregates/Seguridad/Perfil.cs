// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Entidad Perfil
// Agrupa múltiples Roles bajo un perfil de usuario
// Ejemplos: "Perfil Comercial", "Perfil Administración", "Perfil Gerencia"
// ═══════════════════════════════════════════════════════════════════════════════

using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Seguridad;

/// <summary>
/// Perfil: Agrupa Roles para asignar a usuarios.
/// Cada usuario tiene un único perfil.
/// </summary>
public class Perfil : BaseEntity<int>
{
    /// <summary>
    /// Nombre único del perfil (ej: "Perfil Comercial")
    /// </summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>
    /// Descripción del perfil
    /// </summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Indica si el perfil está activo
    /// </summary>
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// Fecha de creación del perfil
    /// </summary>
    public DateTime FechaCreacion { get; private set; } = DateTime.Now;

    /// <summary>
    /// Roles asignados a este perfil (navegación)
    /// </summary>
    public virtual ICollection<PerfilRol> PerfilRoles { get; private set; } = new List<PerfilRol>();

    /// <summary>
    /// Usuarios con este perfil (navegación)
    /// </summary>
    public virtual ICollection<Usuario> Usuarios { get; private set; } = new List<Usuario>();

    // Constructor para EF
    protected Perfil() { }

    public Perfil(string nombre, string descripcion = "")
    {
        SetNombre(nombre);
        Descripcion = descripcion;
    }

    public void SetNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del perfil no puede estar vacío", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void ActualizarDescripcion(string descripcion)
    {
        Descripcion = descripcion ?? string.Empty;
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;

    /// <summary>
    /// Agrega un rol al perfil
    /// </summary>
    public void AgregarRol(Rol rol)
    {
        if (rol == null)
            throw new ArgumentNullException(nameof(rol));

        if (PerfilRoles.Any(pr => pr.RolId == rol.Id))
            return; // Ya existe

        PerfilRoles.Add(new PerfilRol
        {
            Perfil = this,
            PerfilId = this.Id,
            Rol = rol,
            RolId = rol.Id,
            FechaAsignacion = DateTime.Now
        });
    }

    /// <summary>
    /// Remueve un rol del perfil
    /// </summary>
    public void RemoverRol(int rolId)
    {
        var perfilRol = PerfilRoles.FirstOrDefault(pr => pr.RolId == rolId);
        if (perfilRol != null)
        {
            PerfilRoles.Remove(perfilRol);
        }
    }

    /// <summary>
    /// Obtiene todas las capabilities del perfil (a través de sus roles)
    /// </summary>
    public IEnumerable<string> ObtenerCapabilities()
    {
        return PerfilRoles
            .Where(pr => pr.Rol.Activo)
            .SelectMany(pr => pr.Rol.ObtenerCapabilities())
            .Distinct();
    }

    /// <summary>
    /// Verifica si el perfil tiene una capability específica
    /// </summary>
    public bool TieneCapability(string capability)
    {
        return ObtenerCapabilities()
            .Any(c => string.Equals(c, capability, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Tabla intermedia Perfil-Rol (N:M)
/// </summary>
public class PerfilRol
{
    public int PerfilId { get; set; }
    public virtual Perfil Perfil { get; set; } = null!;

    public int RolId { get; set; }
    public virtual Rol Rol { get; set; } = null!;

    /// <summary>
    /// Fecha en que se asignó el rol al perfil
    /// </summary>
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
}
