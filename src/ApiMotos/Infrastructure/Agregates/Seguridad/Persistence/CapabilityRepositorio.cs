using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Seguridad;
using ApiMotos.Infrastructure.Common;

namespace ApiMotos.Infrastructure.Agregates.Seguridad.Persistence;

public class CapabilityRepositorio : ICapabilityRepositorio
{
    private readonly Context _context;

    public CapabilityRepositorio(Context context)
    {
        _context = context;
    }

    public async Task<Capability?> ObtenerPorIdAsync(int id)
    {
        return await _context.Capabilities.FindAsync(id);
    }

    public async Task<Capability?> ObtenerPorNombreAsync(string nombre)
    {
        return await _context.Capabilities
            .FirstOrDefaultAsync(c => c.Nombre == nombre);
    }

    public async Task<IEnumerable<Capability>> ObtenerTodosAsync()
    {
        return await _context.Capabilities
            .OrderBy(c => c.Modulo)
            .ThenBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Capability>> ObtenerActivosAsync()
    {
        return await _context.Capabilities
            .Where(c => c.Activo)
            .OrderBy(c => c.Modulo)
            .ThenBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Capability>> ObtenerPorModuloAsync(string modulo)
    {
        return await _context.Capabilities
            .Where(c => c.Modulo == modulo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<int> CrearAsync(Capability capability)
    {
        _context.Capabilities.Add(capability);
        await _context.SaveChangesAsync();
        return capability.Id;
    }

    public async Task ActualizarAsync(Capability capability)
    {
        _context.Capabilities.Update(capability);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> ObtenerModulosAsync()
    {
        return await _context.Capabilities
            .Select(c => c.Modulo)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();
    }
}
