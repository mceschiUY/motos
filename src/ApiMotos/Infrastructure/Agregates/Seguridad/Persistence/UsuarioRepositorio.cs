using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Seguridad;
using ApiMotos.Infrastructure.Common;

namespace ApiMotos.Infrastructure.Agregates.Seguridad.Persistence;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly Context _context;

    public UsuarioRepositorio(Context context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<Usuario?> ObtenerPorIdConPerfilAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> ObtenerPorUserNameAsync(string userName)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.UserName == userName.ToLowerInvariant());
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
    }

    public async Task<Usuario?> ObtenerParaLoginAsync(string userName)
    {
        return await _context.Usuarios
            .Include(u => u.Perfil)
                .ThenInclude(p => p.PerfilRoles)
                    .ThenInclude(pr => pr.Rol)
                        .ThenInclude(r => r.RolCapabilities)
                            .ThenInclude(rc => rc.Capability)
            .FirstOrDefaultAsync(u => u.UserName == userName.ToLowerInvariant());
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Perfil)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> ObtenerActivosAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Perfil)
            .Where(u => u.Activo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> ObtenerPorPerfilAsync(int perfilId)
    {
        return await _context.Usuarios
            .Where(u => u.PerfilId == perfilId)
            .ToListAsync();
    }

    public async Task<int> CrearAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario.Id;
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExisteUserNameAsync(string userName)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.UserName == userName.ToLowerInvariant());
    }

    public async Task<bool> ExisteEmailAsync(string email)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email.ToLowerInvariant());
    }

    public async Task<IEnumerable<string>> ObtenerCapabilitiesDeUsuarioAsync(int usuarioId)
    {
        var usuario = await ObtenerParaLoginAsync(
            (await _context.Usuarios.FindAsync(usuarioId))?.UserName ?? "");

        return usuario?.ObtenerCapabilities() ?? Enumerable.Empty<string>();
    }
}
