// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Repositorio Perfil
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Domain.Agregates.Seguridad;

public interface IPerfilRepositorio
{
    Task<Perfil?> ObtenerPorIdAsync(int id);
    Task<Perfil?> ObtenerPorIdConRolesAsync(int id);
    Task<Perfil?> ObtenerPorIdConRolesYCapabilitiesAsync(int id);
    Task<Perfil?> ObtenerPorNombreAsync(string nombre);
    Task<IEnumerable<Perfil>> ObtenerTodosAsync();
    Task<IEnumerable<Perfil>> ObtenerActivosAsync();
    Task<int> CrearAsync(Perfil perfil);
    Task ActualizarAsync(Perfil perfil);
    Task AgregarRolAsync(int perfilId, int rolId);
    Task RemoverRolAsync(int perfilId, int rolId);
    Task<IEnumerable<string>> ObtenerCapabilitiesDePerfilAsync(int perfilId);
}
