// =============================================================================
// MODULO DE CONFIGURACION - Repositorio
// =============================================================================

using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Configuracion;
using ApiMotos.Infrastructure.Common;

namespace ApiMotos.Infrastructure.Agregates.Configuracion.Persistence;

public class ConfiguracionRepositorio : IConfiguracionRepositorio
{
    private readonly Context _context;

    public ConfiguracionRepositorio(Context context)
    {
        _context = context;
    }

    public async Task<ConfiguracionSitio?> ObtenerPorIdAsync(int id)
    {
        return await _context.ConfiguracionesSitio.FindAsync(id);
    }

    public async Task<ConfiguracionSitio?> ObtenerPorClaveAsync(string clave)
    {
        return await _context.ConfiguracionesSitio
            .FirstOrDefaultAsync(c => c.Clave == clave.ToLower());
    }

    public async Task<string?> ObtenerValorAsync(string clave)
    {
        var config = await ObtenerPorClaveAsync(clave);
        return config?.Valor;
    }

    public async Task<IEnumerable<ConfiguracionSitio>> ObtenerTodosAsync()
    {
        return await _context.ConfiguracionesSitio
            .Where(c => c.Activo)
            .OrderBy(c => c.Grupo)
            .ThenBy(c => c.Orden)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConfiguracionSitio>> ObtenerPorGrupoAsync(string grupo)
    {
        return await _context.ConfiguracionesSitio
            .Where(c => c.Activo && c.Grupo == grupo.ToLower())
            .OrderBy(c => c.Orden)
            .ToListAsync();
    }

    public async Task<Dictionary<string, string>> ObtenerTodosComoDiccionarioAsync()
    {
        var configs = await ObtenerTodosAsync();
        return configs.ToDictionary(c => c.Clave, c => c.Valor);
    }

    public async Task<int> CrearAsync(ConfiguracionSitio config)
    {
        _context.ConfiguracionesSitio.Add(config);
        await _context.SaveChangesAsync();
        return config.Id;
    }

    public async Task ActualizarAsync(ConfiguracionSitio config)
    {
        _context.ConfiguracionesSitio.Update(config);
        await _context.SaveChangesAsync();
    }

    public async Task ActualizarBatchAsync(IEnumerable<(int id, string valor)> actualizaciones)
    {
        foreach (var (id, valor) in actualizaciones)
        {
            var config = await _context.ConfiguracionesSitio.FindAsync(id);
            if (config != null)
            {
                config.ActualizarValor(valor);
            }
        }
        await _context.SaveChangesAsync();
    }
}
