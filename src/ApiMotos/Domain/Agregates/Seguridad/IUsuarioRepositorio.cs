// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Repositorio Usuario
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Domain.Agregates.Seguridad;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorIdConPerfilAsync(int id);
    Task<Usuario?> ObtenerPorUserNameAsync(string userName);
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<Usuario?> ObtenerParaLoginAsync(string userName);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task<IEnumerable<Usuario>> ObtenerActivosAsync();
    Task<IEnumerable<Usuario>> ObtenerPorPerfilAsync(int perfilId);
    Task<int> CrearAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
    Task<bool> ExisteUserNameAsync(string userName);
    Task<bool> ExisteEmailAsync(string email);
    Task<IEnumerable<string>> ObtenerCapabilitiesDeUsuarioAsync(int usuarioId);
}
