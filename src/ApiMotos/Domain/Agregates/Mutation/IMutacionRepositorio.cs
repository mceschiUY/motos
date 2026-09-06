namespace ApiMotos.Domain.Agregates.Mutation;

/// <summary>
/// Interface del repositorio de mutaciones
/// </summary>
public interface IMutacionRepositorio
{
    /// <summary>
    /// Crea una nueva mutación
    /// </summary>
    Task<int> CrearAsync(Mutacion mutacion);

    /// <summary>
    /// Actualiza una mutación existente
    /// </summary>
    Task ActualizarAsync(Mutacion mutacion);

    /// <summary>
    /// Obtiene una mutación por ID con todos sus impactos y archivos
    /// </summary>
    Task<Mutacion?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Obtiene el historial de mutaciones de un usuario
    /// </summary>
    Task<IEnumerable<Mutacion>> ObtenerPorUsuarioAsync(int usuarioId, int limit = 50);

    /// <summary>
    /// Obtiene las últimas mutaciones del sistema
    /// </summary>
    Task<IEnumerable<Mutacion>> ObtenerUltimasAsync(int cantidad = 20);

    /// <summary>
    /// Obtiene mutaciones por estado
    /// </summary>
    Task<IEnumerable<Mutacion>> ObtenerPorEstadoAsync(MutacionEstado estado);

    /// <summary>
    /// Obtiene mutaciones con filtros y paginación
    /// </summary>
    Task<(IEnumerable<Mutacion> Items, int TotalCount)> ObtenerPaginadoAsync(
        MutacionFiltro filtro,
        int page,
        int pageSize);

    /// <summary>
    /// Agrega un impacto a una mutación
    /// </summary>
    Task AgregarImpactoAsync(MutacionImpacto impacto);

    /// <summary>
    /// Agrega un archivo a una mutación
    /// </summary>
    Task AgregarArchivoAsync(MutacionArchivo archivo);

    /// <summary>
    /// Obtiene los archivos de una mutación (para rollback)
    /// </summary>
    Task<IEnumerable<MutacionArchivo>> ObtenerArchivosPorMutacionAsync(int mutacionId);

    /// <summary>
    /// Cuenta mutaciones por estado (para estadísticas del dashboard)
    /// </summary>
    Task<Dictionary<MutacionEstado, int>> ContarPorEstadoAsync();

    /// <summary>
    /// Obtiene estadísticas de mutaciones
    /// </summary>
    Task<MutacionEstadisticas> ObtenerEstadisticasAsync(DateTime? desde = null, DateTime? hasta = null);
}

/// <summary>
/// Filtro para búsqueda de mutaciones
/// </summary>
public class MutacionFiltro
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? UsuarioId { get; set; }
    public List<MutacionEstado>? Estados { get; set; }
    public string? SearchTerm { get; set; }
    public int? MinCompliance { get; set; }
    public string SortBy { get; set; } = "FechaSolicitud";
    public bool SortDesc { get; set; } = true;
}

/// <summary>
/// Estadísticas de mutaciones para el dashboard
/// </summary>
public class MutacionEstadisticas
{
    public int TotalMutaciones { get; set; }
    public int MutacionesEjecutadas { get; set; }
    public int MutacionesFallidas { get; set; }
    public int MutacionesRevertidas { get; set; }
    public double PromedioCompliance { get; set; }
    public int ArchivosGenerados { get; set; }
    public int ArchivosModificados { get; set; }
}
