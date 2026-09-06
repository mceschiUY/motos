using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Seguridad;
using ApiMotos.Infrastructure.Common;

namespace ApiMotos.Infrastructure.Agregates.Seguridad.Persistence;

public class PerfilRepositorio : IPerfilRepositorio
{
    private readonly Context _context;

    public PerfilRepositorio(Context context)
    {
        _context = context;
    }

    public async Task<Perfil?> ObtenerPorIdAsync(int id)
    {
        return await _context.Perfiles.FindAsync(id);
    }

    public async Task<Perfil?> ObtenerPorIdConRolesAsync(int id)
    {
        return await _context.Perfiles
            .Include(p => p.PerfilRoles)
                .ThenInclude(pr => pr.Rol)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Perfil?> ObtenerPorIdConRolesYCapabilitiesAsync(int id)
    {
        return await _context.Perfiles
            .Include(p => p.PerfilRoles)
                .ThenInclude(pr => pr.Rol)
                    .ThenInclude(r => r.RolCapabilities)
                        .ThenInclude(rc => rc.Capability)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Perfil?> ObtenerPorNombreAsync(string nombre)
    {
        return await _context.Perfiles
            .FirstOrDefaultAsync(p => p.Nombre == nombre);
    }

    public async Task<IEnumerable<Perfil>> ObtenerTodosAsync()
    {
        return await _context.Perfiles
            .Include(p => p.PerfilRoles)
                .ThenInclude(pr => pr.Rol)
            .Include(p => p.Usuarios)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Perfil>> ObtenerActivosAsync()
    {
        return await _context.Perfiles
            .Include(p => p.PerfilRoles)
                .ThenInclude(pr => pr.Rol)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<int> CrearAsync(Perfil perfil)
    {
        _context.Perfiles.Add(perfil);
        await _context.SaveChangesAsync();
        return perfil.Id;
    }

    public async Task ActualizarAsync(Perfil perfil)
    {
        _context.Perfiles.Update(perfil);
        await _context.SaveChangesAsync();
    }

    public async Task AgregarRolAsync(int perfilId, int rolId)
    {
        var existe = await _context.PerfilRoles
            .AnyAsync(pr => pr.PerfilId == perfilId && pr.RolId == rolId);

        if (!existe)
        {
            _context.PerfilRoles.Add(new PerfilRol
            {
                PerfilId = perfilId,
                RolId = rolId,
                FechaAsignacion = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoverRolAsync(int perfilId, int rolId)
    {
        var perfilRol = await _context.PerfilRoles
            .FirstOrDefaultAsync(pr => pr.PerfilId == perfilId && pr.RolId == rolId);

        if (perfilRol != null)
        {
            _context.PerfilRoles.Remove(perfilRol);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<string>> ObtenerCapabilitiesDePerfilAsync(int perfilId)
    {
        var perfil = await ObtenerPorIdConRolesYCapabilitiesAsync(perfilId);
        return perfil?.ObtenerCapabilities() ?? Enumerable.Empty<string>();
    }

    public async Task ReemplazarRolesAsync(int perfilId, IEnumerable<int> rolIds)
    {
        // Eliminar todos los roles actuales
        var actuales = await _context.PerfilRoles
            .Where(pr => pr.PerfilId == perfilId)
            .ToListAsync();

        _context.PerfilRoles.RemoveRange(actuales);

        // Agregar los nuevos
        foreach (var rolId in rolIds)
        {
            _context.PerfilRoles.Add(new PerfilRol
            {
                PerfilId = perfilId,
                RolId = rolId,
                FechaAsignacion = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> ContarUsuariosPorPerfilAsync(int perfilId)
    {
        return await _context.Usuarios.CountAsync(u => u.PerfilId == perfilId);
    }
}
