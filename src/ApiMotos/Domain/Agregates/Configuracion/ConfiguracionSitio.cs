// =============================================================================
// MODULO DE CONFIGURACION - Entidad ConfiguracionSitio
// Almacena configuraciones clave-valor del sitio (nombre, logo, contacto, redes)
// =============================================================================

using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Configuracion;

/// <summary>
/// ConfiguracionSitio: Almacena configuraciones del sitio como pares clave-valor.
/// Cada configuracion tiene un tipo (string, email, url, image) y pertenece a un grupo.
/// </summary>
public class ConfiguracionSitio : BaseEntity<int>
{
    /// <summary>
    /// Clave unica de la configuracion (ej: "sitio.nombre", "empresa.email")
    /// </summary>
    public string Clave { get; private set; } = string.Empty;

    /// <summary>
    /// Valor de la configuracion
    /// </summary>
    public string Valor { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo de dato: string, email, url, image
    /// </summary>
    public string Tipo { get; private set; } = "string";

    /// <summary>
    /// Grupo al que pertenece: general, contacto, redes, legal
    /// </summary>
    public string Grupo { get; private set; } = "general";

    /// <summary>
    /// Orden de visualizacion dentro del grupo
    /// </summary>
    public int Orden { get; private set; } = 0;

    /// <summary>
    /// Descripcion para mostrar en el admin
    /// </summary>
    public string? Descripcion { get; private set; }

    /// <summary>
    /// Indica si la configuracion esta activa
    /// </summary>
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// Fecha de creacion
    /// </summary>
    public DateTime FechaCreacion { get; private set; } = DateTime.Now;

    /// <summary>
    /// Fecha de ultima actualizacion
    /// </summary>
    public DateTime? FechaActualizacion { get; private set; }

    // Constructor para EF
    protected ConfiguracionSitio() { }

    public ConfiguracionSitio(string clave, string valor, string tipo = "string", string grupo = "general", string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(clave))
            throw new ArgumentException("La clave no puede estar vacia", nameof(clave));

        Clave = clave.Trim().ToLower();
        Valor = valor ?? string.Empty;
        Tipo = tipo;
        Grupo = grupo;
        Descripcion = descripcion;
    }

    /// <summary>
    /// Actualiza el valor de la configuracion
    /// </summary>
    public void ActualizarValor(string valor)
    {
        Valor = valor ?? string.Empty;
        FechaActualizacion = DateTime.Now;
    }

    /// <summary>
    /// Actualiza la descripcion
    /// </summary>
    public void ActualizarDescripcion(string? descripcion)
    {
        Descripcion = descripcion;
        FechaActualizacion = DateTime.Now;
    }

    /// <summary>
    /// Actualiza el orden de visualizacion
    /// </summary>
    public void ActualizarOrden(int orden)
    {
        Orden = orden;
        FechaActualizacion = DateTime.Now;
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;
}
