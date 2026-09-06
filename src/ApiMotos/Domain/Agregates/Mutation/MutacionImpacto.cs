using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Mutation;

/// <summary>
/// Representa un impacto estructural de una mutación sobre una capa de la arquitectura.
/// Describe qué cambios se realizarán en cada capa (Domain, Application, Infrastructure, etc.)
/// </summary>
public class MutacionImpacto : BaseEntity<int>
{
    /// <summary>
    /// ID de la mutación padre
    /// </summary>
    public int MutacionId { get; private set; }

    /// <summary>
    /// Capa de la arquitectura afectada
    /// </summary>
    public CapaArquitectura Capa { get; private set; }

    /// <summary>
    /// Tipo de impacto (crear, modificar, eliminar)
    /// </summary>
    public TipoImpacto Tipo { get; private set; }

    /// <summary>
    /// Descripción legible del impacto
    /// Ejemplo: "Crear nueva entidad NotificacionDeuda en Domain"
    /// </summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo afectado (relativa al proyecto)
    /// </summary>
    public string? RutaArchivo { get; private set; }

    /// <summary>
    /// Código generado para este impacto
    /// </summary>
    public string? CodigoGenerado { get; private set; }

    /// <summary>
    /// Lenguaje del código (csharp, typescript, html, scss, sql)
    /// </summary>
    public string? Lenguaje { get; private set; }

    /// <summary>
    /// Orden de ejecución del impacto (para dependencias)
    /// </summary>
    public int Orden { get; private set; }

    // Navegación
    public Mutacion? Mutacion { get; private set; }

    // Constructor privado para EF
    private MutacionImpacto() { }

    /// <summary>
    /// Crea un nuevo impacto estructural
    /// </summary>
    public static MutacionImpacto Crear(
        int mutacionId,
        CapaArquitectura capa,
        TipoImpacto tipo,
        string descripcion,
        string? rutaArchivo = null,
        string? codigoGenerado = null,
        string? lenguaje = null,
        int orden = 0)
    {
        return new MutacionImpacto
        {
            MutacionId = mutacionId,
            Capa = capa,
            Tipo = tipo,
            Descripcion = descripcion,
            RutaArchivo = rutaArchivo,
            CodigoGenerado = codigoGenerado,
            Lenguaje = lenguaje,
            Orden = orden
        };
    }

    /// <summary>
    /// Actualiza el código generado (después de refinamiento)
    /// </summary>
    public void ActualizarCodigo(string codigo, string lenguaje)
    {
        CodigoGenerado = codigo;
        Lenguaje = lenguaje;
    }
}
