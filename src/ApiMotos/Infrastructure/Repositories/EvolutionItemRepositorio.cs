using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Evolution;
using ApiMotos.Domain.Repositories;
using ApiMotos.Infrastructure.Common;

namespace ApiMotos.Infrastructure.Repositories;

public class EvolutionItemRepositorio : IEvolutionItemRepositorio
{
    private readonly Context _context;

    public EvolutionItemRepositorio(Context context)
    {
        _context = context;
    }

    public async Task<List<EvolutionItem>> GetAllAsync()
    {
        return await _context.EvolutionItems
            .Where(e => e.Estado != EvolutionItemEstado.Archived)
            .OrderBy(e => e.Estado)
            .ThenBy(e => e.Orden)
            .ToListAsync();
    }

    public async Task<List<EvolutionItem>> GetByEstadoAsync(EvolutionItemEstado estado)
    {
        return await _context.EvolutionItems
            .Where(e => e.Estado == estado)
            .OrderBy(e => e.Orden)
            .ToListAsync();
    }

    public async Task<EvolutionItem?> GetByIdAsync(int id)
    {
        return await _context.EvolutionItems.FindAsync(id);
    }

    public async Task<int> CreateAsync(EvolutionItem item)
    {
        _context.EvolutionItems.Add(item);
        await _context.SaveChangesAsync();
        return item.Id;
    }

    public async Task UpdateAsync(EvolutionItem item)
    {
        _context.EvolutionItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrdenAsync(int id, int orden)
    {
        var item = await _context.EvolutionItems.FindAsync(id);
        if (item != null)
        {
            item.ActualizarOrden(orden);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteFeatureAsync(string featureId, string? entityName)
    {
        return await _context.EvolutionItems
            .AnyAsync(e => e.FeatureId == featureId
                        && e.EntidadRelacionada == entityName
                        && e.Estado != EvolutionItemEstado.Archived);
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.EvolutionItems.FindAsync(id);
        if (item != null)
        {
            item.Archivar();
            await _context.SaveChangesAsync();
        }
    }
}
