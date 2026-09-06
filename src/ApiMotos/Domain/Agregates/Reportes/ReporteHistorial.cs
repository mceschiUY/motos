using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Reportes;

/// <summary>
/// Historial de reportes generados
/// </summary>
public class ReporteHistorial : BaseEntity<int>
{
    /// <summary>
    /// ID del reporte programado (null si fue generación manual)
    /// </summary>
    public int? ReporteProgramadoId { get; private set; }

    /// <summary>
    /// Tipo de reporte generado
    /// </summary>
    public string TipoReporte { get; private set; } = string.Empty;

    /// <summary>
    /// Entidad/módulo del reporte
    /// </summary>
    public string? Entidad { get; private set; }

    /// <summary>
    /// Formato del archivo generado
    /// </summary>
    public string Formato { get; private set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de generación
    /// </summary>
    public DateTime FechaGeneracion { get; private set; }

    /// <summary>
    /// Usuario que generó el reporte (null si fue automático)
    /// </summary>
    public int? GeneradoPor { get; private set; }

    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long? TamanioBytes { get; private set; }

    /// <summary>
    /// Cantidad de registros exportados
    /// </summary>
    public int Registros { get; private set; }

    /// <summary>
    /// Estado del reporte: Completado, Error, EnProceso
    /// </summary>
    public string Estado { get; private set; } = string.Empty;

    /// <summary>
    /// Mensaje de error si falló
    /// </summary>
    public string? ErrorMensaje { get; private set; }

    /// <summary>
    /// Duración de generación en milisegundos
    /// </summary>
    public int? DuracionMs { get; private set; }

    /// <summary>
    /// Parámetros/filtros usados (JSON)
    /// </summary>
    public string? Parametros { get; private set; }

    // Constructor privado para EF
    private ReporteHistorial() { }

    /// <summary>
    /// Crea un nuevo registro de historial
    /// </summary>
    public static ReporteHistorial Crear(
        int? reporteProgramadoId,
        string tipoReporte,
        string? entidad,
        string formato,
        int? generadoPor,
        string? parametros)
    {
        return new ReporteHistorial
        {
            ReporteProgramadoId = reporteProgramadoId,
            TipoReporte = tipoReporte,
            Entidad = entidad,
            Formato = formato,
            FechaGeneracion = DateTime.UtcNow,
            GeneradoPor = generadoPor,
            Estado = EstadoReporte.EnProceso,
            Parametros = parametros
        };
    }

    /// <summary>
    /// Marca el reporte como completado
    /// </summary>
    public void MarcarCompletado(int registros, long tamanioBytes, int duracionMs)
    {
        Estado = EstadoReporte.Completado;
        Registros = registros;
        TamanioBytes = tamanioBytes;
        DuracionMs = duracionMs;
        ErrorMensaje = null;
    }

    /// <summary>
    /// Marca el reporte como fallido
    /// </summary>
    public void MarcarError(string errorMensaje, int duracionMs)
    {
        Estado = EstadoReporte.Error;
        ErrorMensaje = errorMensaje;
        DuracionMs = duracionMs;
    }
}

/// <summary>
/// Estados posibles de un reporte
/// </summary>
public static class EstadoReporte
{
    public const string EnProceso = "EnProceso";
    public const string Completado = "Completado";
    public const string Error = "Error";
}
