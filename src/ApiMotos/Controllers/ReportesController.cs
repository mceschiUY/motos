using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using ApiMotos.Domain.Agregates.Reportes;
using ApiMotos.Infrastructure.Services;

using Microsoft.AspNetCore.Authorization;
namespace ApiMotos.Controllers;

/// <summary>
/// Controller para exportación de datos y gestión de reportes
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly IReporteProgramadoRepositorio _repoProgramados;
    private readonly IReporteHistorialRepositorio _repoHistorial;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(
        IExportService exportService,
        IReporteProgramadoRepositorio repoProgramados,
        IReporteHistorialRepositorio repoHistorial,
        IConfiguration configuration,
        ILogger<ReportesController> logger)
    {
        _exportService = exportService;
        _repoProgramados = repoProgramados;
        _repoHistorial = repoHistorial;
        _configuration = configuration;
        _logger = logger;
    }

    // =============================================================================
    // EXPORTACIÓN GENÉRICA
    // =============================================================================

    /// <summary>
    /// Exporta datos de una entidad a Excel o CSV
    /// </summary>
    [HttpPost("exportar")]
    public async Task<IActionResult> Exportar([FromBody] ExportRequest request)
    {
        var startTime = DateTime.UtcNow;
        var userId = GetCurrentUserId();

        // Crear registro de historial
        var historial = ReporteHistorial.Crear(
            reporteProgramadoId: null,
            tipoReporte: TipoReporte.Listado,
            entidad: request.Entidad,
            formato: request.Formato,
            generadoPor: userId,
            parametros: JsonSerializer.Serialize(request.Filtros)
        );

        try
        {
            // Ejecutar consulta SQL dinámica
            var data = await ExecuteQueryAsync(request);

            if (!data.Any())
            {
                return BadRequest(new { message = "No hay datos para exportar con los filtros seleccionados" });
            }

            byte[] fileBytes;
            string contentType;
            string extension;

            var options = new ExportOptions
            {
                FileName = $"{request.Entidad}_{DateTime.Now:yyyyMMdd_HHmmss}",
                SheetName = request.Entidad,
                Title = request.Titulo ?? $"Reporte de {request.Entidad}",
                Columns = request.Columnas,
                ColumnHeaders = request.ColumnasHeaders,
                IncludeTimestamp = true
            };

            switch (request.Formato.ToLower())
            {
                case "excel":
                case "xlsx":
                    fileBytes = await _exportService.ExportDynamicToExcelAsync(data, options);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    extension = "xlsx";
                    break;

                case "csv":
                    fileBytes = await _exportService.ExportDynamicToCsvAsync(data, options);
                    contentType = "text/csv";
                    extension = "csv";
                    break;

                default:
                    return BadRequest(new { message = $"Formato '{request.Formato}' no soportado. Use 'excel' o 'csv'." });
            }

            // Registrar éxito
            var durationMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            historial.MarcarCompletado(data.Count(), fileBytes.Length, durationMs);
            await _repoHistorial.CrearAsync(historial);

            _logger.LogInformation(
                "[Reportes] Exportación exitosa: {Entidad}, {Formato}, {Registros} registros, {Size} bytes",
                request.Entidad, request.Formato, data.Count(), fileBytes.Length);

            return File(fileBytes, contentType, $"{options.FileName}.{extension}");
        }
        catch (ArgumentException ex)
        {
            // Entidad/columna fuera de la whitelist del esquema: pedido inválido, no error interno
            var durationMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            historial.MarcarError(ex.Message, durationMs);
            await _repoHistorial.CrearAsync(historial);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            var durationMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            historial.MarcarError(ex.Message, durationMs);
            await _repoHistorial.CrearAsync(historial);

            _logger.LogError(ex, "[Reportes] Error exportando {Entidad}", request.Entidad);
            return StatusCode(500, new { message = "Error al generar el reporte", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene las columnas disponibles de una entidad para configurar exportación
    /// </summary>
    [HttpGet("columnas/{entidad}")]
    public async Task<ActionResult<IEnumerable<ColumnInfo>>> GetColumnas(string entidad)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DbConnection");
            await using var connection = new SqlConnection(connectionString);

            // Obtener información de columnas de la tabla
            var sql = @"
                SELECT
                    COLUMN_NAME as Name,
                    DATA_TYPE as DataType,
                    IS_NULLABLE as IsNullable,
                    CHARACTER_MAXIMUM_LENGTH as MaxLength
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @TableName
                ORDER BY ORDINAL_POSITION";

            var columnas = await connection.QueryAsync<ColumnInfo>(sql, new { TableName = entidad });

            return Ok(columnas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Reportes] Error obteniendo columnas de {Entidad}", entidad);
            return StatusCode(500, new { message = "Error al obtener columnas", error = ex.Message });
        }
    }

    // =============================================================================
    // HISTORIAL DE REPORTES
    // =============================================================================

    /// <summary>
    /// Obtiene el historial de reportes generados
    /// </summary>
    [HttpGet("historial")]
    public async Task<ActionResult<IEnumerable<ReporteHistorialDto>>> GetHistorial(
        [FromQuery] int limit = 50,
        [FromQuery] string? entidad = null)
    {
        IEnumerable<ReporteHistorial> historial;

        if (!string.IsNullOrEmpty(entidad))
        {
            historial = await _repoHistorial.ObtenerPorEntidadAsync(entidad, limit);
        }
        else
        {
            historial = await _repoHistorial.ObtenerUltimosAsync(limit);
        }

        return Ok(historial.Select(h => new ReporteHistorialDto(h)));
    }

    /// <summary>
    /// Obtiene historial de reportes del usuario actual
    /// </summary>
    [HttpGet("historial/mis-reportes")]
    public async Task<ActionResult<IEnumerable<ReporteHistorialDto>>> GetMisReportes([FromQuery] int limit = 50)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var historial = await _repoHistorial.ObtenerPorUsuarioAsync(userId.Value, limit);
        return Ok(historial.Select(h => new ReporteHistorialDto(h)));
    }

    /// <summary>
    /// Obtiene estadísticas de reportes generados
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<ActionResult<ReporteEstadisticasDto>> GetEstadisticas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.UtcNow.AddDays(-30);
        var fechaHasta = hasta ?? DateTime.UtcNow;

        var stats = await _repoHistorial.ObtenerEstadisticasAsync(fechaDesde, fechaHasta);

        return Ok(new ReporteEstadisticasDto
        {
            TotalGenerados = stats.TotalGenerados,
            Exitosos = stats.Exitosos,
            Fallidos = stats.Fallidos,
            TotalBytes = stats.TotalBytes,
            TotalRegistros = stats.TotalRegistros,
            PorFormato = stats.PorFormato,
            PorEntidad = stats.PorEntidad,
            Desde = fechaDesde,
            Hasta = fechaHasta
        });
    }

    // =============================================================================
    // REPORTES PROGRAMADOS
    // =============================================================================

    /// <summary>
    /// Obtiene todos los reportes programados
    /// </summary>
    [HttpGet("programados")]
    public async Task<ActionResult<IEnumerable<ReporteProgramadoDto>>> GetProgramados()
    {
        var reportes = await _repoProgramados.ObtenerTodosAsync();
        return Ok(reportes.Select(r => new ReporteProgramadoDto(r)));
    }

    /// <summary>
    /// Obtiene un reporte programado por ID
    /// </summary>
    [HttpGet("programados/{id}")]
    public async Task<ActionResult<ReporteProgramadoDto>> GetProgramado(int id)
    {
        var reporte = await _repoProgramados.ObtenerPorIdAsync(id);
        if (reporte == null)
        {
            return NotFound();
        }
        return Ok(new ReporteProgramadoDto(reporte));
    }

    /// <summary>
    /// Crea un nuevo reporte programado
    /// </summary>
    [HttpPost("programados")]
    public async Task<ActionResult<int>> CrearProgramado([FromBody] CrearReporteProgramadoRequest request)
    {
        var userId = GetCurrentUserId() ?? 0;

        var reporte = ReporteProgramado.Crear(
            nombre: request.Nombre,
            tipoReporte: request.TipoReporte,
            entidad: request.Entidad,
            formato: request.Formato,
            cronExpression: request.CronExpression,
            destinatarios: request.Destinatarios != null ? JsonSerializer.Serialize(request.Destinatarios) : null,
            parametros: request.Parametros != null ? JsonSerializer.Serialize(request.Parametros) : null,
            columnas: request.Columnas != null ? JsonSerializer.Serialize(request.Columnas) : null,
            creadoPor: userId
        );

        var id = await _repoProgramados.CrearAsync(reporte);

        _logger.LogInformation("[Reportes] Reporte programado creado: {Id} - {Nombre}", id, request.Nombre);

        return CreatedAtAction(nameof(GetProgramado), new { id }, id);
    }

    /// <summary>
    /// Activa un reporte programado
    /// </summary>
    [HttpPost("programados/{id}/activar")]
    public async Task<IActionResult> ActivarProgramado(int id)
    {
        var reporte = await _repoProgramados.ObtenerPorIdAsync(id);
        if (reporte == null)
        {
            return NotFound();
        }

        reporte.Activar();
        await _repoProgramados.ActualizarAsync(reporte);

        return Ok();
    }

    /// <summary>
    /// Desactiva un reporte programado
    /// </summary>
    [HttpPost("programados/{id}/desactivar")]
    public async Task<IActionResult> DesactivarProgramado(int id)
    {
        var reporte = await _repoProgramados.ObtenerPorIdAsync(id);
        if (reporte == null)
        {
            return NotFound();
        }

        reporte.Desactivar();
        await _repoProgramados.ActualizarAsync(reporte);

        return Ok();
    }

    /// <summary>
    /// Elimina un reporte programado
    /// </summary>
    [HttpDelete("programados/{id}")]
    public async Task<IActionResult> EliminarProgramado(int id)
    {
        await _repoProgramados.EliminarAsync(id);
        return NoContent();
    }

    // =============================================================================
    // ADMINISTRACIÓN
    // =============================================================================

    /// <summary>
    /// Limpia historial de reportes antiguo
    /// </summary>
    [HttpDelete("historial/limpiar")]
    public async Task<ActionResult<int>> LimpiarHistorial([FromQuery] int diasRetención = 90)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(-diasRetención);
        var eliminados = await _repoHistorial.EliminarAntiguosAsync(fechaLimite);

        _logger.LogInformation(
            "[Reportes] Limpieza de historial: {Eliminados} registros anteriores a {Fecha}",
            eliminados, fechaLimite);

        return Ok(eliminados);
    }

    /// <summary>
    /// Obtiene los formatos de exportación disponibles
    /// </summary>
    [HttpGet("formatos")]
    public ActionResult<IEnumerable<FormatoInfo>> GetFormatos()
    {
        return Ok(new[]
        {
            new FormatoInfo { Codigo = FormatoExport.Excel, Nombre = "Excel", Extension = "xlsx", ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            new FormatoInfo { Codigo = FormatoExport.Csv, Nombre = "CSV", Extension = "csv", ContentType = "text/csv" }
        });
    }

    // =============================================================================
    // MÉTODOS PRIVADOS
    // =============================================================================

    private async Task<IEnumerable<Dictionary<string, object?>>> ExecuteQueryAsync(ExportRequest request)
    {
        var connectionString = _configuration.GetConnectionString("DbConnection");
        await using var connection = new SqlConnection(connectionString);

        // WHITELIST contra el esquema REAL: tabla y columnas deben existir en la base.
        // Entidad/Columnas/OrdenarPor/Filtros.Key viajaban interpolados al SQL — los
        // corchetes no protegen (un ']' en el nombre rompe el identificador → inyección).
        var existeTabla = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @t",
            new { t = request.Entidad });
        if (existeTabla == 0)
            throw new ArgumentException($"La entidad '{request.Entidad}' no existe");

        var columnasReales = (await connection.QueryAsync<string>(
            "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @t",
            new { t = request.Entidad })).ToHashSet(StringComparer.OrdinalIgnoreCase);

        static string Escapar(string id) => "[" + id.Replace("]", "]]") + "]";

        foreach (var c in request.Columnas ?? new List<string>())
            if (!columnasReales.Contains(c))
                throw new ArgumentException($"Columna inválida: '{c}'");

        if (!string.IsNullOrEmpty(request.OrdenarPor) && !columnasReales.Contains(request.OrdenarPor))
            throw new ArgumentException($"Columna de orden inválida: '{request.OrdenarPor}'");

        var selectColumns = request.Columnas != null && request.Columnas.Count > 0
            ? string.Join(", ", request.Columnas.Select(Escapar))
            : "*";

        var sql = $"SELECT {selectColumns} FROM {Escapar(request.Entidad)}";

        // Filtros: claves validadas contra el esquema, valores SIEMPRE parametrizados
        DynamicParameters? parameters = null;
        if (request.Filtros != null && request.Filtros.Count > 0)
        {
            var whereConditions = new List<string>();
            parameters = new DynamicParameters();
            var i = 0;

            foreach (var filtro in request.Filtros)
            {
                if (!columnasReales.Contains(filtro.Key))
                    throw new ArgumentException($"Columna de filtro inválida: '{filtro.Key}'");

                var paramName = $"@p{i++}";
                whereConditions.Add($"{Escapar(filtro.Key)} = {paramName}");
                parameters.Add(paramName, filtro.Value);
            }

            sql += " WHERE " + string.Join(" AND ", whereConditions);
        }

        // Ordenamiento (columna ya validada; la dirección es un booleano propio)
        if (!string.IsNullOrEmpty(request.OrdenarPor))
        {
            sql += $" ORDER BY {Escapar(request.OrdenarPor)} {(request.OrdenDesc ? "DESC" : "ASC")}";
        }

        // Limitar registros
        if (request.MaxRegistros > 0)
        {
            sql = sql.Replace("SELECT ", $"SELECT TOP {request.MaxRegistros} ");
        }

        var results = await connection.QueryAsync(sql, parameters);

        return results.Select(row =>
        {
            var dict = new Dictionary<string, object?>();
            foreach (var prop in (IDictionary<string, object>)row)
            {
                dict[prop.Key] = prop.Value;
            }
            return dict;
        });
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

// =============================================================================
// REQUEST/RESPONSE DTOs
// =============================================================================

public class ExportRequest
{
    /// <summary>
    /// Nombre de la tabla/entidad a exportar
    /// </summary>
    public string Entidad { get; set; } = string.Empty;

    /// <summary>
    /// Formato de exportación: excel, csv
    /// </summary>
    public string Formato { get; set; } = "excel";

    /// <summary>
    /// Título opcional del reporte
    /// </summary>
    public string? Titulo { get; set; }

    /// <summary>
    /// Columnas a incluir (null = todas)
    /// </summary>
    public List<string>? Columnas { get; set; }

    /// <summary>
    /// Mapeo de nombres de columna para headers
    /// </summary>
    public Dictionary<string, string>? ColumnasHeaders { get; set; }

    /// <summary>
    /// Filtros a aplicar (columna -> valor)
    /// </summary>
    public Dictionary<string, object>? Filtros { get; set; }

    /// <summary>
    /// Columna para ordenar
    /// </summary>
    public string? OrdenarPor { get; set; }

    /// <summary>
    /// Ordenar descendente
    /// </summary>
    public bool OrdenDesc { get; set; }

    /// <summary>
    /// Máximo de registros a exportar (0 = sin límite)
    /// </summary>
    public int MaxRegistros { get; set; } = 10000;
}

public class CrearReporteProgramadoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string TipoReporte { get; set; } = "Listado";
    public string? Entidad { get; set; }
    public string Formato { get; set; } = "excel";
    public string? CronExpression { get; set; }
    public List<string>? Destinatarios { get; set; }
    public Dictionary<string, object>? Parametros { get; set; }
    public List<string>? Columnas { get; set; }
}

public class ReporteProgramadoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoReporte { get; set; } = string.Empty;
    public string? Entidad { get; set; }
    public string Formato { get; set; } = string.Empty;
    public string? CronExpression { get; set; }
    public List<string>? Destinatarios { get; set; }
    public Dictionary<string, object>? Parametros { get; set; }
    public List<string>? Columnas { get; set; }
    public bool Activo { get; set; }
    public DateTime? UltimaEjecucion { get; set; }
    public DateTime? ProximaEjecucion { get; set; }
    public int CreadoPor { get; set; }
    public DateTime FechaCreacion { get; set; }

    public ReporteProgramadoDto() { }

    public ReporteProgramadoDto(ReporteProgramado r)
    {
        Id = r.Id;
        Nombre = r.Nombre;
        TipoReporte = r.TipoReporte;
        Entidad = r.Entidad;
        Formato = r.Formato;
        CronExpression = r.CronExpression;
        Activo = r.Activo;
        UltimaEjecucion = r.UltimaEjecucion;
        ProximaEjecucion = r.ProximaEjecucion;
        CreadoPor = r.CreadoPor;
        FechaCreacion = r.FechaCreacion;

        if (!string.IsNullOrEmpty(r.Destinatarios))
            Destinatarios = JsonSerializer.Deserialize<List<string>>(r.Destinatarios);
        if (!string.IsNullOrEmpty(r.Parametros))
            Parametros = JsonSerializer.Deserialize<Dictionary<string, object>>(r.Parametros);
        if (!string.IsNullOrEmpty(r.Columnas))
            Columnas = JsonSerializer.Deserialize<List<string>>(r.Columnas);
    }
}

public class ReporteHistorialDto
{
    public int Id { get; set; }
    public int? ReporteProgramadoId { get; set; }
    public string TipoReporte { get; set; } = string.Empty;
    public string? Entidad { get; set; }
    public string Formato { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public int? GeneradoPor { get; set; }
    public long? TamanioBytes { get; set; }
    public int Registros { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? ErrorMensaje { get; set; }
    public int? DuracionMs { get; set; }

    public ReporteHistorialDto() { }

    public ReporteHistorialDto(ReporteHistorial h)
    {
        Id = h.Id;
        ReporteProgramadoId = h.ReporteProgramadoId;
        TipoReporte = h.TipoReporte;
        Entidad = h.Entidad;
        Formato = h.Formato;
        FechaGeneracion = h.FechaGeneracion;
        GeneradoPor = h.GeneradoPor;
        TamanioBytes = h.TamanioBytes;
        Registros = h.Registros;
        Estado = h.Estado;
        ErrorMensaje = h.ErrorMensaje;
        DuracionMs = h.DuracionMs;
    }
}

public class ReporteEstadisticasDto
{
    public int TotalGenerados { get; set; }
    public int Exitosos { get; set; }
    public int Fallidos { get; set; }
    public long TotalBytes { get; set; }
    public int TotalRegistros { get; set; }
    public Dictionary<string, int> PorFormato { get; set; } = new();
    public Dictionary<string, int> PorEntidad { get; set; } = new();
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string IsNullable { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
}

public class FormatoInfo
{
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
