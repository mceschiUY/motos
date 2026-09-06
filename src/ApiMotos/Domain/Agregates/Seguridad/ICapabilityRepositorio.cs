// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Repositorio Capability
// ═══════════════════════════════════════════════════════════════════════════════

namespace ApiMotos.Domain.Agregates.Seguridad;

public interface ICapabilityRepositorio
{
    Task<Capability?> ObtenerPorIdAsync(int id);
    Task<Capability?> ObtenerPorNombreAsync(string nombre);
    Task<IEnumerable<Capability>> ObtenerTodosAsync();
    Task<IEnumerable<Capability>> ObtenerActivosAsync();
    Task<IEnumerable<Capability>> ObtenerPorModuloAsync(string modulo);
    Task<int> CrearAsync(Capability capability);
    Task ActualizarAsync(Capability capability);
    Task<IEnumerable<string>> ObtenerModulosAsync();
}
