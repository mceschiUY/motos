using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Seguridad;
using ApiMotos.Infrastructure.Common;

namespace ApiMotos.Infrastructure.Agregates.Seguridad.Persistence;

public class RolRepositorio : IRolRepositorio
{
    private readonly Context _context;

    public RolRepositorio(Context context)
    {
        _context = context;
    }

    public async Task<Rol?> ObtenerPorIdAsync(int id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Rol?> ObtenerPorIdConCapabilitiesAsync(int id)
    {
        return await _context.Roles
            .Include(r => r.RolCapabilities)
                .ThenInclude(rc => rc.Capability)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Rol?> ObtenerPorNombreAsync(string nombre)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Nombre == nombre);
    }

    public async Task<IEnumerable<Rol>> ObtenerTodosAsync()
    {
        return await _context.Roles
            .Include(r => r.RolCapabilities)
                .ThenInclude(rc => rc.Capability)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Rol>> ObtenerActivosAsync()
    {
        return await _context.Roles
            .Include(r => r.RolCapabilities)
                .ThenInclude(rc => rc.Capability)
            .Where(r => r.Activo)
            .OrderBy(r => r.Nombre)
            .ToListAsync();
    }

    public async Task<int> CrearAsync(Rol rol)
    {
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();
        return rol.Id;
    }

    public async Task ActualizarAsync(Rol rol)
    {
        _context.Roles.Update(rol);
        await _context.SaveChangesAsync();
    }

    public async Task AgregarCapabilityAsync(int rolId, int capabilityId)
    {
        var existe = await _context.RolCapabilities
            .AnyAsync(rc => rc.RolId == rolId && rc.CapabilityId == capabilityId);

        if (!existe)
        {
            _context.RolCapabilities.Add(new RolCapability
            {
                RolId = rolId,
                CapabilityId = capabilityId,
                FechaAsignacion = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoverCapabilityAsync(int rolId, int capabilityId)
    {
        var rolCapability = await _context.RolCapabilities
            .FirstOrDefaultAsync(rc => rc.RolId == rolId && rc.CapabilityId == capabilityId);

        if (rolCapability != null)
        {
            _context.RolCapabilities.Remove(rolCapability);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<string>> ObtenerCapabilitiesDeRolAsync(int rolId)
    {
        return await _context.RolCapabilities
            .Where(rc => rc.RolId == rolId)
            .Include(rc => rc.Capability)
            .Select(rc => rc.Capability.Nombre)
            .ToListAsync();
    }

    public async Task ReemplazarCapabilitiesAsync(int rolId, IEnumerable<int> capabilityIds)
    {
        // Eliminar todas las capabilities actuales
        var actuales = await _context.RolCapabilities
            .Where(rc => rc.RolId == rolId)
            .ToListAsync();

        _context.RolCapabilities.RemoveRange(actuales);

        // Agregar las nuevas
        foreach (var capId in capabilityIds)
        {
            _context.RolCapabilities.Add(new RolCapability
            {
                RolId = rolId,
                CapabilityId = capId,
                FechaAsignacion = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
    }
}
