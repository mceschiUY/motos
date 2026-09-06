using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Reportes;

namespace ApiMotos.Infrastructure.Agregates.Reportes.Persistence;

public class ReporteProgramadoRepositorio : IReporteProgramadoRepositorio
{
    private readonly ReportesContext _context;

    public ReporteProgramadoRepositorio(ReportesContext context)
    {
        _context = context;
    }

    public async Task<int> CrearAsync(ReporteProgramado reporte)
    {
        await _context.ReportesProgramados.AddAsync(reporte);
        await _context.SaveChangesAsync();
        return reporte.Id;
    }

    public async Task<ReporteProgramado?> ObtenerPorIdAsync(int id)
    {
        return await _context.ReportesProgramados
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<ReporteProgramado>> ObtenerTodosAsync()
    {
        return await _context.ReportesProgramados
            .AsNoTracking()
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReporteProgramado>> ObtenerActivosAsync()
    {
        return await _context.ReportesProgramados
            .AsNoTracking()
            .Where(r => r.Activo)
            .OrderBy(r => r.ProximaEjecucion)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReporteProgramado>> ObtenerPorEntidadAsync(string entidad)
    {
        return await _context.ReportesProgramados
            .AsNoTracking()
            .Where(r => r.Entidad == entidad)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<ReporteProgramado>> ObtenerPorUsuarioAsync(int userId)
    {
        return await _context.ReportesProgramados
            .AsNoTracking()
            .Where(r => r.CreadoPor == userId)
            .OrderByDescending(r => r.FechaCreacion)
            .ToListAsync();
    }

    public async Task ActualizarAsync(ReporteProgramado reporte)
    {
        _context.ReportesProgramados.Update(reporte);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int id)
    {
        var reporte = await _context.ReportesProgramados.FindAsync(id);
        if (reporte != null)
        {
            _context.ReportesProgramados.Remove(reporte);
            await _context.SaveChangesAsync();
        }
    }
}
