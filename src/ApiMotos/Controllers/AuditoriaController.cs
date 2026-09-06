using Microsoft.AspNetCore.Mvc;
using ApiMotos.Domain.Agregates.Auditoria;

using Microsoft.AspNetCore.Authorization;
namespace ApiMotos.Controllers;

/// <summary>
/// Controller para consulta y gestión de registros de auditoría
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditLogRepositorio _repo;
    private readonly ILogger<AuditoriaController> _logger;

    public AuditoriaController(
        IAuditLogRepositorio repo,
        ILogger<AuditoriaController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // =============================================================================
    // CONSULTAS
    // =============================================================================

    /// <summary>
    /// Obtiene registros de auditoría con filtros y paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AuditLogPagedResponse>> GetAll(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int? userId,
        [FromQuery] string? userName,
        [FromQuery] string? actions,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] bool? success,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string sortBy = "Timestamp",
        [FromQuery] bool sortDesc = true)
    {
        var filtro = new AuditLogFiltro
        {
            Desde = desde,
            Hasta = hasta,
            UserId = userId,
            UserName = userName,
            Actions = string.IsNullOrEmpty(actions) ? null : actions.Split(',').ToList(),
            EntityType = entityType,
            EntityId = entityId,
            Success = success,
            SearchTerm = search,
            SortBy = sortBy,
            SortDesc = sortDesc
        };

        var (items, totalCount) = await _repo.ObtenerPaginadoAsync(filtro, page, pageSize);

        return Ok(new AuditLogPagedResponse
        {
            Items = items.Select(l => new AuditLogDto(l)),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    /// <summary>
    /// Obtiene los últimos N registros de auditoría
    /// </summary>
    [HttpGet("recientes")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetRecientes([FromQuery] int cantidad = 50)
    {
        var logs = await _repo.ObtenerUltimosAsync(cantidad);
        return Ok(logs.Select(l => new AuditLogDto(l)));
    }

    /// <summary>
    /// Obtiene registros de auditoría de un usuario específico
    /// </summary>
    [HttpGet("usuario/{userId}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetByUsuario(int userId, [FromQuery] int limit = 100)
    {
        var logs = await _repo.ObtenerPorUsuarioAsync(userId, limit);
        return Ok(logs.Select(l => new AuditLogDto(l)));
    }

    /// <summary>
    /// Obtiene el historial de una entidad específica
    /// </summary>
    [HttpGet("entidad/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetByEntidad(string entityType, string entityId)
    {
        var logs = await _repo.ObtenerPorEntidadAsync(entityType, entityId);
        return Ok(logs.Select(l => new AuditLogDto(l)));
    }

    /// <summary>
    /// Obtiene registros por tipo de acción
    /// </summary>
    [HttpGet("accion/{action}")]
    public async Task<ActionResult<IEnumerable<AuditLogDto>>> GetByAccion(string action, [FromQuery] int limit = 100)
    {
        var logs = await _repo.ObtenerPorAccionAsync(action, limit);
        return Ok(logs.Select(l => new AuditLogDto(l)));
    }

    /// <summary>
    /// Obtiene lista de tipos de acción disponibles
    /// </summary>
    [HttpGet("acciones")]
    public ActionResult<IEnumerable<string>> GetAcciones()
    {
        return Ok(new[]
        {
            AuditAction.Create,
            AuditAction.Update,
            AuditAction.Delete,
            AuditAction.Read,
            AuditAction.Login,
            AuditAction.Logout,
            AuditAction.LoginFailed,
            AuditAction.Export
        });
    }

    // =============================================================================
    // ESTADÍSTICAS
    // =============================================================================

    /// <summary>
    /// Obtiene estadísticas de auditoría
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<ActionResult<AuditStatsDto>> GetEstadisticas(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
    {
        var fechaDesde = desde ?? DateTime.UtcNow.AddDays(-30);
        var fechaHasta = hasta ?? DateTime.UtcNow;

        var porAccion = await _repo.ContarPorAccionAsync(fechaDesde, fechaHasta);
        var porUsuario = await _repo.ContarPorUsuarioAsync(fechaDesde, fechaHasta, 10);
        var total = await _repo.ContarTotalAsync();

        return Ok(new AuditStatsDto
        {
            TotalRegistros = total,
            Desde = fechaDesde,
            Hasta = fechaHasta,
            PorAccion = porAccion,
            TopUsuarios = porUsuario.Select(kv => new UserActivityDto
            {
                UserId = kv.Key,
                Acciones = kv.Value
            }).ToList()
        });
    }

    // =============================================================================
    // ADMINISTRACIÓN
    // =============================================================================

    /// <summary>
    /// Elimina registros de auditoría antiguos
    /// </summary>
    [HttpDelete("limpiar")]
    public async Task<ActionResult<int>> Limpiar([FromQuery] int diasRetención = 90)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(-diasRetención);
        var eliminados = await _repo.EliminarAntiguosAsync(fechaLimite);

        _logger.LogInformation(
            "Limpieza de auditoría: {Eliminados} registros anteriores a {Fecha}",
            eliminados, fechaLimite);

        return Ok(eliminados);
    }
}

// =============================================================================
// DTOs
// =============================================================================

public class AuditLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public int? DurationMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public AuditLogDto() { }

    public AuditLogDto(AuditLog log)
    {
        Id = log.Id;
        Timestamp = log.Timestamp;
        UserId = log.UserId;
        UserName = log.UserName;
        Action = log.Action;
        EntityType = log.EntityType;
        EntityId = log.EntityId;
        OldValues = log.OldValues;
        NewValues = log.NewValues;
        IpAddress = log.IpAddress;
        UserAgent = log.UserAgent;
        RequestPath = log.RequestPath;
        DurationMs = log.DurationMs;
        Success = log.Success;
        ErrorMessage = log.ErrorMessage;
    }
}

public class AuditLogPagedResponse
{
    public IEnumerable<AuditLogDto> Items { get; set; } = Enumerable.Empty<AuditLogDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class AuditStatsDto
{
    public int TotalRegistros { get; set; }
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public Dictionary<string, int> PorAccion { get; set; } = new();
    public List<UserActivityDto> TopUsuarios { get; set; } = new();
}

public class UserActivityDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public int Acciones { get; set; }
}
