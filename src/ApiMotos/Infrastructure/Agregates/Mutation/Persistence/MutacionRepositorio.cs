using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Infrastructure.Agregates.Mutation.Persistence;

public class MutacionRepositorio : IMutacionRepositorio
{
    private readonly MutacionContext _context;

    public MutacionRepositorio(MutacionContext context)
    {
        _context = context;
    }

    public async Task<int> CrearAsync(Mutacion mutacion)
    {
        await _context.Mutaciones.AddAsync(mutacion);
        await _context.SaveChangesAsync();
        return mutacion.Id;
    }

    public async Task ActualizarAsync(Mutacion mutacion)
    {
        _context.Mutaciones.Update(mutacion);
        await _context.SaveChangesAsync();
    }

    public async Task<Mutacion?> ObtenerPorIdAsync(int id)
    {
        return await _context.Mutaciones
            .Include(m => m.Impactos)
            .Include(m => m.Archivos)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Mutacion>> ObtenerPorUsuarioAsync(int usuarioId, int limit = 50)
    {
        return await _context.Mutaciones
            .AsNoTracking()
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.FechaSolicitud)
            .Take(limit)
            .Include(m => m.Archivos)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mutacion>> ObtenerUltimasAsync(int cantidad = 20)
    {
        return await _context.Mutaciones
            .AsNoTracking()
            .OrderByDescending(m => m.FechaSolicitud)
            .Take(cantidad)
            .Include(m => m.Archivos)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mutacion>> ObtenerPorEstadoAsync(MutacionEstado estado)
    {
        return await _context.Mutaciones
            .AsNoTracking()
            .Where(m => m.Estado == estado)
            .OrderByDescending(m => m.FechaSolicitud)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Mutacion> Items, int TotalCount)> ObtenerPaginadoAsync(
        MutacionFiltro filtro,
        int page,
        int pageSize)
    {
        var query = _context.Mutaciones.AsNoTracking();

        // Aplicar filtros
        if (filtro.Desde.HasValue)
            query = query.Where(m => m.FechaSolicitud >= filtro.Desde.Value);

        if (filtro.Hasta.HasValue)
            query = query.Where(m => m.FechaSolicitud <= filtro.Hasta.Value);

        if (filtro.UsuarioId.HasValue)
            query = query.Where(m => m.UsuarioId == filtro.UsuarioId.Value);

        if (filtro.Estados != null && filtro.Estados.Count > 0)
            query = query.Where(m => filtro.Estados.Contains(m.Estado));

        if (filtro.MinCompliance.HasValue)
            query = query.Where(m => m.ArchitectureCompliance >= filtro.MinCompliance.Value);

        if (!string.IsNullOrEmpty(filtro.SearchTerm))
        {
            var term = filtro.SearchTerm.ToLower();
            query = query.Where(m =>
                m.SolicitudOriginal.ToLower().Contains(term) ||
                (m.ResumenTecnico != null && m.ResumenTecnico.ToLower().Contains(term)) ||
                (m.UsuarioNombre != null && m.UsuarioNombre.ToLower().Contains(term)));
        }

        // Contar total
        var totalCount = await query.CountAsync();

        // Ordenar
        query = filtro.SortDesc
            ? query.OrderByDescending(m => EF.Property<object>(m, filtro.SortBy))
            : query.OrderBy(m => EF.Property<object>(m, filtro.SortBy));

        // Paginar
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Archivos)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AgregarImpactoAsync(MutacionImpacto impacto)
    {
        await _context.MutacionImpactos.AddAsync(impacto);
        await _context.SaveChangesAsync();
    }

    public async Task AgregarArchivoAsync(MutacionArchivo archivo)
    {
        await _context.MutacionArchivos.AddAsync(archivo);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<MutacionArchivo>> ObtenerArchivosPorMutacionAsync(int mutacionId)
    {
        return await _context.MutacionArchivos
            .AsNoTracking()
            .Where(a => a.MutacionId == mutacionId)
            .OrderBy(a => a.FechaOperacion)
            .ToListAsync();
    }

    public async Task<Dictionary<MutacionEstado, int>> ContarPorEstadoAsync()
    {
        return await _context.Mutaciones
            .AsNoTracking()
            .GroupBy(m => m.Estado)
            .Select(g => new { Estado = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Estado, x => x.Count);
    }

    public async Task<MutacionEstadisticas> ObtenerEstadisticasAsync(DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Mutaciones.AsNoTracking();

        if (desde.HasValue)
            query = query.Where(m => m.FechaSolicitud >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(m => m.FechaSolicitud <= hasta.Value);

        var mutaciones = await query.ToListAsync();

        var archivosQuery = _context.MutacionArchivos.AsNoTracking();
        if (desde.HasValue)
            archivosQuery = archivosQuery.Where(a => a.FechaOperacion >= desde.Value);
        if (hasta.HasValue)
            archivosQuery = archivosQuery.Where(a => a.FechaOperacion <= hasta.Value);

        var archivos = await archivosQuery.ToListAsync();

        return new MutacionEstadisticas
        {
            TotalMutaciones = mutaciones.Count,
            MutacionesEjecutadas = mutaciones.Count(m => m.Estado == MutacionEstado.Ejecutada),
            MutacionesFallidas = mutaciones.Count(m => m.Estado == MutacionEstado.Fallida),
            MutacionesRevertidas = mutaciones.Count(m => m.Estado == MutacionEstado.Revertida),
            PromedioCompliance = mutaciones.Where(m => m.ArchitectureCompliance.HasValue)
                .Select(m => m.ArchitectureCompliance!.Value)
                .DefaultIfEmpty(0)
                .Average(),
            ArchivosGenerados = archivos.Count(a => a.Operacion == TipoImpacto.Crear),
            ArchivosModificados = archivos.Count(a => a.Operacion == TipoImpacto.Modificar)
        };
    }
}
