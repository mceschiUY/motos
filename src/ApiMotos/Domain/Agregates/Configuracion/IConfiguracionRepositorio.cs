// =============================================================================
// MODULO DE CONFIGURACION - Interface del Repositorio
// =============================================================================

namespace ApiMotos.Domain.Agregates.Configuracion;

/// <summary>
/// Interface del repositorio de configuraciones del sitio
/// </summary>
public interface IConfiguracionRepositorio
{
    /// <summary>
    /// Obtiene una configuracion por su ID
    /// </summary>
    Task<ConfiguracionSitio?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Obtiene una configuracion por su clave
    /// </summary>
    Task<ConfiguracionSitio?> ObtenerPorClaveAsync(string clave);

    /// <summary>
    /// Obtiene el valor de una configuracion por su clave
    /// </summary>
    Task<string?> ObtenerValorAsync(string clave);

    /// <summary>
    /// Obtiene todas las configuraciones activas
    /// </summary>
    Task<IEnumerable<ConfiguracionSitio>> ObtenerTodosAsync();

    /// <summary>
    /// Obtiene configuraciones por grupo
    /// </summary>
    Task<IEnumerable<ConfiguracionSitio>> ObtenerPorGrupoAsync(string grupo);

    /// <summary>
    /// Obtiene todas las configuraciones como diccionario clave-valor
    /// </summary>
    Task<Dictionary<string, string>> ObtenerTodosComoDiccionarioAsync();

    /// <summary>
    /// Crea una nueva configuracion
    /// </summary>
    Task<int> CrearAsync(ConfiguracionSitio config);

    /// <summary>
    /// Actualiza una configuracion existente
    /// </summary>
    Task ActualizarAsync(ConfiguracionSitio config);

    /// <summary>
    /// Actualiza multiples configuraciones en batch
    /// </summary>
    Task ActualizarBatchAsync(IEnumerable<(int id, string valor)> actualizaciones);
}
