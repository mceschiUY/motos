using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Auditoria;

namespace ApiMotos.Infrastructure.Agregates.Auditoria.Persistence;

public class AuditLogRepositorio : IAuditLogRepositorio
{
    private readonly AuditLogContext _context;

    public AuditLogRepositorio(AuditLogContext context)
    {
        _context = context;
    }

    public async Task<int> CrearAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
        return log.Id;
    }

    public async Task CrearBatchAsync(IEnumerable<AuditLog> logs)
    {
        await _context.AuditLogs.AddRangeAsync(logs);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> ObtenerPorFechaAsync(DateTime desde, DateTime hasta, int limit = 1000)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= desde && l.Timestamp <= hasta)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> ObtenerPorUsuarioAsync(int userId, int limit = 100)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> ObtenerPorEntidadAsync(string entityType, string entityId)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> ObtenerPorAccionAsync(string action, int limit = 100)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.Action == action)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> ObtenerUltimosAsync(int cantidad)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(l => l.Timestamp)
            .Take(cantidad)
            .ToListAsync();
    }

    public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> ObtenerPaginadoAsync(
        AuditLogFiltro filtro,
        int page,
        int pageSize)
    {
        var query = _context.AuditLogs.AsNoTracking();

        // Aplicar filtros
        if (filtro.Desde.HasValue)
            query = query.Where(l => l.Timestamp >= filtro.Desde.Value);

        if (filtro.Hasta.HasValue)
            query = query.Where(l => l.Timestamp <= filtro.Hasta.Value);

        if (filtro.UserId.HasValue)
            query = query.Where(l => l.UserId == filtro.UserId.Value);

        if (!string.IsNullOrEmpty(filtro.UserName))
            query = query.Where(l => l.UserName != null && l.UserName.Contains(filtro.UserName));

        if (filtro.Actions != null && filtro.Actions.Count > 0)
            query = query.Where(l => filtro.Actions.Contains(l.Action));

        if (!string.IsNullOrEmpty(filtro.EntityType))
            query = query.Where(l => l.EntityType == filtro.EntityType);

        if (!string.IsNullOrEmpty(filtro.EntityId))
            query = query.Where(l => l.EntityId == filtro.EntityId);

        if (filtro.Success.HasValue)
            query = query.Where(l => l.Success == filtro.Success.Value);

        if (!string.IsNullOrEmpty(filtro.SearchTerm))
        {
            var term = filtro.SearchTerm.ToLower();
            query = query.Where(l =>
                (l.UserName != null && l.UserName.ToLower().Contains(term)) ||
                l.EntityType.ToLower().Contains(term) ||
                (l.EntityId != null && l.EntityId.ToLower().Contains(term)) ||
                l.Action.ToLower().Contains(term));
        }

        // Contar total
        var totalCount = await query.CountAsync();

        // Ordenar
        query = filtro.SortDesc
            ? query.OrderByDescending(l => EF.Property<object>(l, filtro.SortBy))
            : query.OrderBy(l => EF.Property<object>(l, filtro.SortBy));

        // Paginar
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Dictionary<string, int>> ContarPorAccionAsync(DateTime desde, DateTime hasta)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= desde && l.Timestamp <= hasta)
            .GroupBy(l => l.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Action, x => x.Count);
    }

    public async Task<Dictionary<int, int>> ContarPorUsuarioAsync(DateTime desde, DateTime hasta, int topN = 10)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(l => l.Timestamp >= desde && l.Timestamp <= hasta && l.UserId.HasValue)
            .GroupBy(l => l.UserId!.Value)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(topN)
            .ToDictionaryAsync(x => x.UserId, x => x.Count);
    }

    public async Task<int> EliminarAntiguosAsync(DateTime antesDe)
    {
        var antiguos = await _context.AuditLogs
            .Where(l => l.Timestamp < antesDe)
            .ToListAsync();

        _context.AuditLogs.RemoveRange(antiguos);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> ContarTotalAsync()
    {
        return await _context.AuditLogs.CountAsync();
    }
}
