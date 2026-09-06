namespace ApiMotos.Domain.Agregates.Reportes;

/// <summary>
/// Interface del repositorio de reportes programados
/// </summary>
public interface IReporteProgramadoRepositorio
{
    Task<int> CrearAsync(ReporteProgramado reporte);
    Task<ReporteProgramado?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<ReporteProgramado>> ObtenerTodosAsync();
    Task<IEnumerable<ReporteProgramado>> ObtenerActivosAsync();
    Task<IEnumerable<ReporteProgramado>> ObtenerPorEntidadAsync(string entidad);
    Task<IEnumerable<ReporteProgramado>> ObtenerPorUsuarioAsync(int userId);
    Task ActualizarAsync(ReporteProgramado reporte);
    Task EliminarAsync(int id);
}

/// <summary>
/// Interface del repositorio de historial de reportes
/// </summary>
public interface IReporteHistorialRepositorio
{
    Task<int> CrearAsync(ReporteHistorial historial);
    Task<ReporteHistorial?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<ReporteHistorial>> ObtenerUltimosAsync(int cantidad);
    Task<IEnumerable<ReporteHistorial>> ObtenerPorUsuarioAsync(int userId, int limit = 50);
    Task<IEnumerable<ReporteHistorial>> ObtenerPorEntidadAsync(string entidad, int limit = 50);
    Task<IEnumerable<ReporteHistorial>> ObtenerPorReporteProgramadoAsync(int reporteProgramadoId);
    Task ActualizarAsync(ReporteHistorial historial);
    Task<int> EliminarAntiguosAsync(DateTime antesDe);

    /// <summary>
    /// Obtiene estadísticas de reportes generados
    /// </summary>
    Task<ReporteEstadisticas> ObtenerEstadisticasAsync(DateTime desde, DateTime hasta);
}

/// <summary>
/// Estadísticas de reportes
/// </summary>
public class ReporteEstadisticas
{
    public int TotalGenerados { get; set; }
    public int Exitosos { get; set; }
    public int Fallidos { get; set; }
    public long TotalBytes { get; set; }
    public int TotalRegistros { get; set; }
    public Dictionary<string, int> PorFormato { get; set; } = new();
    public Dictionary<string, int> PorEntidad { get; set; } = new();
}
