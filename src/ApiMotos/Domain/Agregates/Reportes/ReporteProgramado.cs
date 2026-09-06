using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Reportes;

/// <summary>
/// Entidad para reportes programados que se ejecutan automáticamente
/// </summary>
public class ReporteProgramado : BaseEntity<int>
{
    /// <summary>
    /// Nombre descriptivo del reporte
    /// </summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo de reporte (módulo o específico)
    /// </summary>
    public string TipoReporte { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre de la entidad/módulo (ej: "Cliente", "Documento")
    /// </summary>
    public string? Entidad { get; private set; }

    /// <summary>
    /// Formato de salida: excel, csv, pdf
    /// </summary>
    public string Formato { get; private set; } = "excel";

    /// <summary>
    /// Expresión CRON para la programación (ej: "0 8 * * 1" = lunes 8am)
    /// </summary>
    public string? CronExpression { get; private set; }

    /// <summary>
    /// Lista de emails destinatarios (JSON array)
    /// </summary>
    public string? Destinatarios { get; private set; }

    /// <summary>
    /// Parámetros y filtros del reporte (JSON)
    /// </summary>
    public string? Parametros { get; private set; }

    /// <summary>
    /// Columnas a incluir en el reporte (JSON array)
    /// </summary>
    public string? Columnas { get; private set; }

    /// <summary>
    /// Indica si el reporte está activo
    /// </summary>
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// Última vez que se ejecutó
    /// </summary>
    public DateTime? UltimaEjecucion { get; private set; }

    /// <summary>
    /// Próxima ejecución programada
    /// </summary>
    public DateTime? ProximaEjecucion { get; private set; }

    /// <summary>
    /// Usuario que creó el reporte programado
    /// </summary>
    public int CreadoPor { get; private set; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime FechaCreacion { get; private set; }

    // Constructor privado para EF
    private ReporteProgramado() { }

    /// <summary>
    /// Crea un nuevo reporte programado
    /// </summary>
    public static ReporteProgramado Crear(
        string nombre,
        string tipoReporte,
        string? entidad,
        string formato,
        string? cronExpression,
        string? destinatarios,
        string? parametros,
        string? columnas,
        int creadoPor)
    {
        return new ReporteProgramado
        {
            Nombre = nombre,
            TipoReporte = tipoReporte,
            Entidad = entidad,
            Formato = formato,
            CronExpression = cronExpression,
            Destinatarios = destinatarios,
            Parametros = parametros,
            Columnas = columnas,
            Activo = true,
            CreadoPor = creadoPor,
            FechaCreacion = DateTime.UtcNow
        };
    }

    public void Activar() => Activo = true;
    public void Desactivar() => Activo = false;

    public void ActualizarUltimaEjecucion(DateTime fecha)
    {
        UltimaEjecucion = fecha;
    }

    public void ActualizarProximaEjecucion(DateTime? fecha)
    {
        ProximaEjecucion = fecha;
    }

    public void Modificar(
        string nombre,
        string formato,
        string? cronExpression,
        string? destinatarios,
        string? parametros,
        string? columnas)
    {
        Nombre = nombre;
        Formato = formato;
        CronExpression = cronExpression;
        Destinatarios = destinatarios;
        Parametros = parametros;
        Columnas = columnas;
    }
}

/// <summary>
/// Tipos de reporte disponibles
/// </summary>
public static class TipoReporte
{
    public const string Listado = "Listado";
    public const string ResumenPeriodo = "ResumenPeriodo";
    public const string Estadisticas = "Estadisticas";
    public const string ActividadUsuario = "ActividadUsuario";
    public const string Personalizado = "Personalizado";
}

/// <summary>
/// Formatos de exportación
/// </summary>
public static class FormatoExport
{
    public const string Excel = "excel";
    public const string Csv = "csv";
    public const string Pdf = "pdf";
}
