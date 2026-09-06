using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Reportes;

namespace ApiMotos.Infrastructure.Agregates.Reportes.Persistence;

public class ReporteHistorialRepositorio : IReporteHistorialRepositorio
{
    private readonly ReportesContext _context;

    public ReporteHistorialRepositorio(ReportesContext context)
    {
        _context = context;
    }

    public async Task<int> CrearAsync(ReporteHistorial historial)
    {
        await _context.ReportesHistorial.AddAsync(historial);
        await _context.SaveChangesAsync();
        return historial.Id;
    }

    public async Task<ReporteHistorial?> ObtenerPorIdAsync(int id)
    {
        return await _context.ReportesHistorial
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IEnumerable<ReporteHistorial>> ObtenerUltimosAsync(int cantidad)
    {
        return await _context.ReportesHistorial
            .AsNoTracking()
            .OrderByDescending(h => h.FechaGeneracion)
            .Take(cantidad)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReporteHistorial>> ObtenerPorUsuarioAsync(int userId, int limit = 50)
    {
        return await _context.ReportesHistorial
            .AsNoTracking()
            .Where(h => h.GeneradoPor == userId)
            .OrderByDescending(h => h.FechaGeneracion)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReporteHistorial>> ObtenerPorEntidadAsync(string entidad, int limit = 50)
    {
        return await _context.ReportesHistorial
            .AsNoTracking()
            .Where(h => h.Entidad == entidad)
            .OrderByDescending(h => h.FechaGeneracion)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReporteHistorial>> ObtenerPorReporteProgramadoAsync(int reporteProgramadoId)
    {
        return await _context.ReportesHistorial
            .AsNoTracking()
            .Where(h => h.ReporteProgramadoId == reporteProgramadoId)
            .OrderByDescending(h => h.FechaGeneracion)
            .ToListAsync();
    }

    public async Task ActualizarAsync(ReporteHistorial historial)
    {
        _context.ReportesHistorial.Update(historial);
        await _context.SaveChangesAsync();
    }

    public async Task<int> EliminarAntiguosAsync(DateTime antesDe)
    {
        var antiguos = await _context.ReportesHistorial
            .Where(h => h.FechaGeneracion < antesDe)
            .ToListAsync();

        _context.ReportesHistorial.RemoveRange(antiguos);
        return await _context.SaveChangesAsync();
    }

    public async Task<ReporteEstadisticas> ObtenerEstadisticasAsync(DateTime desde, DateTime hasta)
    {
        var reportes = await _context.ReportesHistorial
            .AsNoTracking()
            .Where(h => h.FechaGeneracion >= desde && h.FechaGeneracion <= hasta)
            .ToListAsync();

        var estadisticas = new ReporteEstadisticas
        {
            TotalGenerados = reportes.Count,
            Exitosos = reportes.Count(r => r.Estado == EstadoReporte.Completado),
            Fallidos = reportes.Count(r => r.Estado == EstadoReporte.Error),
            TotalBytes = reportes.Sum(r => r.TamanioBytes ?? 0),
            TotalRegistros = reportes.Sum(r => r.Registros),
            PorFormato = reportes
                .GroupBy(r => r.Formato)
                .ToDictionary(g => g.Key, g => g.Count()),
            PorEntidad = reportes
                .Where(r => !string.IsNullOrEmpty(r.Entidad))
                .GroupBy(r => r.Entidad!)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return estadisticas;
    }
}
