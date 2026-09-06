using ApiMotos.Domain.Agregates.Evolution;

namespace ApiMotos.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar items del Evolution Board.
/// </summary>
public interface IEvolutionItemRepositorio
{
    Task<List<EvolutionItem>> GetAllAsync();
    Task<List<EvolutionItem>> GetByEstadoAsync(EvolutionItemEstado estado);
    Task<EvolutionItem?> GetByIdAsync(int id);
    Task<int> CreateAsync(EvolutionItem item);
    Task UpdateAsync(EvolutionItem item);
    Task UpdateOrdenAsync(int id, int orden);
    Task<bool> ExisteFeatureAsync(string featureId, string? entityName);
    Task DeleteAsync(int id);
}
