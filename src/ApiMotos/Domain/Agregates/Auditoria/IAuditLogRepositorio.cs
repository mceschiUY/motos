namespace ApiMotos.Domain.Agregates.Auditoria;

/// <summary>
/// Interface del repositorio de auditoría
/// </summary>
public interface IAuditLogRepositorio
{
    /// <summary>
    /// Crea un nuevo registro de auditoría
    /// </summary>
    Task<int> CrearAsync(AuditLog log);

    /// <summary>
    /// Crea múltiples registros en batch (para alto volumen)
    /// </summary>
    Task CrearBatchAsync(IEnumerable<AuditLog> logs);

    /// <summary>
    /// Obtiene registros por rango de fecha
    /// </summary>
    Task<IEnumerable<AuditLog>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta, int limit = 1000);

    /// <summary>
    /// Obtiene registros de un usuario específico
    /// </summary>
    Task<IEnumerable<AuditLog>> ObtenerPorUsuarioAsync(int userId, int limit = 100);

    /// <summary>
    /// Obtiene historial de una entidad específica
    /// </summary>
    Task<IEnumerable<AuditLog>> ObtenerPorEntidadAsync(string entityType, string entityId);

    /// <summary>
    /// Obtiene registros por tipo de acción
    /// </summary>
    Task<IEnumerable<AuditLog>> ObtenerPorAccionAsync(string action, int limit = 100);

    /// <summary>
    /// Obtiene los últimos N registros
    /// </summary>
    Task<IEnumerable<AuditLog>> ObtenerUltimosAsync(int cantidad);

    /// <summary>
    /// Obtiene registros con filtros combinados y paginación
    /// </summary>
    Task<(IEnumerable<AuditLog> Items, int TotalCount)> ObtenerPaginadoAsync(
        AuditLogFiltro filtro,
        int page,
        int pageSize);

    /// <summary>
    /// Cuenta registros por tipo de acción (para estadísticas)
    /// </summary>
    Task<Dictionary<string, int>> ContarPorAccionAsync(DateTime desde, DateTime hasta);

    /// <summary>
    /// Cuenta registros por usuario (para estadísticas)
    /// </summary>
    Task<Dictionary<int, int>> ContarPorUsuarioAsync(DateTime desde, DateTime hasta, int topN = 10);

    /// <summary>
    /// Elimina registros más antiguos que la fecha especificada
    /// </summary>
    Task<int> EliminarAntiguosAsync(DateTime antesde);

    /// <summary>
    /// Obtiene el conteo total de registros
    /// </summary>
    Task<int> ContarTotalAsync();
}

/// <summary>
/// Filtro para búsqueda de registros de auditoría
/// </summary>
public class AuditLogFiltro
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public List<string>? Actions { get; set; }
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public bool? Success { get; set; }
    public string? SearchTerm { get; set; }
    public string SortBy { get; set; } = "Timestamp";
    public bool SortDesc { get; set; } = true;
}
