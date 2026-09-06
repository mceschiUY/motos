using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Mutation;

/// <summary>
/// Representa un archivo físico creado o modificado por una mutación.
/// Almacena información para poder hacer rollback si es necesario.
/// </summary>
public class MutacionArchivo : BaseEntity<int>
{
    /// <summary>
    /// ID de la mutación padre
    /// </summary>
    public int MutacionId { get; private set; }

    /// <summary>
    /// Ruta completa del archivo
    /// </summary>
    public string RutaCompleta { get; private set; } = string.Empty;

    /// <summary>
    /// Ruta relativa al proyecto
    /// </summary>
    public string RutaRelativa { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo de operación realizada
    /// </summary>
    public TipoImpacto Operacion { get; private set; }

    /// <summary>
    /// Contenido original del archivo (para rollback en modificaciones)
    /// </summary>
    public string? ContenidoOriginal { get; private set; }

    /// <summary>
    /// Contenido nuevo del archivo
    /// </summary>
    public string? ContenidoNuevo { get; private set; }

    /// <summary>
    /// Hash del contenido para verificar integridad
    /// </summary>
    public string? HashContenido { get; private set; }

    /// <summary>
    /// Fecha de creación/modificación del archivo
    /// </summary>
    public DateTime FechaOperacion { get; private set; }

    /// <summary>
    /// Indica si el archivo fue restaurado (rollback)
    /// </summary>
    public bool Restaurado { get; private set; }

    /// <summary>
    /// Fecha de restauración si aplica
    /// </summary>
    public DateTime? FechaRestauracion { get; private set; }

    // Navegación
    public Mutacion? Mutacion { get; private set; }

    // Constructor privado para EF
    private MutacionArchivo() { }

    /// <summary>
    /// Registra la creación de un archivo nuevo
    /// </summary>
    public static MutacionArchivo RegistrarCreacion(
        int mutacionId,
        string rutaCompleta,
        string rutaRelativa,
        string contenidoNuevo)
    {
        return new MutacionArchivo
        {
            MutacionId = mutacionId,
            RutaCompleta = rutaCompleta,
            RutaRelativa = rutaRelativa,
            Operacion = TipoImpacto.Crear,
            ContenidoOriginal = null,
            ContenidoNuevo = contenidoNuevo,
            HashContenido = ComputeHash(contenidoNuevo),
            FechaOperacion = DateTime.UtcNow,
            Restaurado = false
        };
    }

    /// <summary>
    /// Registra la modificación de un archivo existente
    /// </summary>
    public static MutacionArchivo RegistrarModificacion(
        int mutacionId,
        string rutaCompleta,
        string rutaRelativa,
        string contenidoOriginal,
        string contenidoNuevo)
    {
        return new MutacionArchivo
        {
            MutacionId = mutacionId,
            RutaCompleta = rutaCompleta,
            RutaRelativa = rutaRelativa,
            Operacion = TipoImpacto.Modificar,
            ContenidoOriginal = contenidoOriginal,
            ContenidoNuevo = contenidoNuevo,
            HashContenido = ComputeHash(contenidoNuevo),
            FechaOperacion = DateTime.UtcNow,
            Restaurado = false
        };
    }

    /// <summary>
    /// Registra la eliminación de un archivo
    /// </summary>
    public static MutacionArchivo RegistrarEliminacion(
        int mutacionId,
        string rutaCompleta,
        string rutaRelativa,
        string contenidoOriginal)
    {
        return new MutacionArchivo
        {
            MutacionId = mutacionId,
            RutaCompleta = rutaCompleta,
            RutaRelativa = rutaRelativa,
            Operacion = TipoImpacto.Eliminar,
            ContenidoOriginal = contenidoOriginal,
            ContenidoNuevo = null,
            HashContenido = null,
            FechaOperacion = DateTime.UtcNow,
            Restaurado = false
        };
    }

    /// <summary>
    /// Marca el archivo como restaurado (rollback completado)
    /// </summary>
    public void MarcarRestaurado()
    {
        Restaurado = true;
        FechaRestauracion = DateTime.UtcNow;
    }

    private static string ComputeHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
