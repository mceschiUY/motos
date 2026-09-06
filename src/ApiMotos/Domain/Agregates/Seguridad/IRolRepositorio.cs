// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Repositorio Rol
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Domain.Agregates.Seguridad;

public interface IRolRepositorio
{
    Task<Rol?> ObtenerPorIdAsync(int id);
    Task<Rol?> ObtenerPorIdConCapabilitiesAsync(int id);
    Task<Rol?> ObtenerPorNombreAsync(string nombre);
    Task<IEnumerable<Rol>> ObtenerTodosAsync();
    Task<IEnumerable<Rol>> ObtenerActivosAsync();
    Task<int> CrearAsync(Rol rol);
    Task ActualizarAsync(Rol rol);
    Task AgregarCapabilityAsync(int rolId, int capabilityId);
    Task RemoverCapabilityAsync(int rolId, int capabilityId);
    Task<IEnumerable<string>> ObtenerCapabilitiesDeRolAsync(int rolId);
}
