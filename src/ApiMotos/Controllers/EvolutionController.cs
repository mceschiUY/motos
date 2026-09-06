using Microsoft.AspNetCore.Mvc;
using ApiMotos.Application.Evolution;
using ApiMotos.Domain.Agregates.Evolution;
using ApiMotos.Domain.Repositories;

using Microsoft.AspNetCore.Authorization;
namespace ApiMotos.Controllers;

/// <summary>
/// Controller para el Evolution Board - Kanban de gobernanza de evoluciones.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EvolutionController : ControllerBase
{
    private readonly IEvolutionItemRepositorio _repo;
    private readonly ILogger<EvolutionController> _logger;

    public EvolutionController(
        IEvolutionItemRepositorio repo,
        ILogger<EvolutionController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los items del board agrupados por estado.
    /// </summary>
    [HttpGet("board")]
    public async Task<ActionResult<EvolutionBoardDto>> GetBoard()
    {
        var items = await _repo.GetAllAsync();
        var dtos = items.Select(i => new EvolutionItemDto(i)).ToList();

        var board = new EvolutionBoardDto
        {
            Backlog = dtos.Where(i => i.Estado == "Backlog").ToList(),
            Approved = dtos.Where(i => i.Estado == "Approved").ToList(),
            Building = dtos.Where(i => i.Estado == "Building").ToList(),
            Review = dtos.Where(i => i.Estado == "Review").ToList(),
            Deployed = dtos.Where(i => i.Estado == "Deployed").ToList(),
            Failed = dtos.Where(i => i.Estado == "Failed").ToList(),
            Rejected = dtos.Where(i => i.Estado == "Rejected").ToList()
        };

        return Ok(board);
    }

    /// <summary>
    /// Crea un item manualmente en el board.
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<int>> CreateItem([FromBody] CreateEvolutionItemRequest request)
    {
        var item = EvolutionItem.Crear(
            request.Titulo,
            request.Descripcion,
            request.BusinessValue,
            request.Prioridad,
            request.EntidadRelacionada,
            request.Categoria,
            request.PromptMutation,
            request.FeatureId,
            request.Icon
        );

        var id = await _repo.CreateAsync(item);
        _logger.LogInformation("Evolution item creado: {Titulo} (ID: {Id})", request.Titulo, id);
        return Ok(id);
    }

    /// <summary>
    /// Importa propuestas desde el Evolution Hub al board (deduplica por featureId + entityName).
    /// </summary>
    [HttpPost("items/import")]
    public async Task<ActionResult> ImportProposals([FromBody] ImportProposalsRequest request)
    {
        var imported = 0;
        var skipped = 0;

        foreach (var proposal in request.Proposals)
        {
            var exists = await _repo.ExisteFeatureAsync(proposal.FeatureId, proposal.EntityName);
            if (exists)
            {
                skipped++;
                continue;
            }

            var item = EvolutionItem.Crear(
                proposal.FeatureName,
                proposal.Description,
                proposal.BusinessValue,
                proposal.Priority,
                proposal.EntityName,
                proposal.Category,
                proposal.Prompt,
                proposal.FeatureId,
                proposal.Icon
            );

            await _repo.CreateAsync(item);
            imported++;
        }

        _logger.LogInformation("Proposals importadas: {Imported} nuevas, {Skipped} duplicadas", imported, skipped);
        return Ok(new { imported, skipped });
    }

    /// <summary>
    /// Cambia el estado de un item (drag & drop entre columnas).
    /// </summary>
    [HttpPut("items/{id}/estado")]
    public async Task<ActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoRequest request)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        if (!Enum.TryParse<EvolutionItemEstado>(request.Estado, true, out var nuevoEstado))
            return BadRequest(new { Error = $"Estado inválido: {request.Estado}" });

        if (nuevoEstado == EvolutionItemEstado.Failed && !string.IsNullOrEmpty(request.ErrorMessage))
        {
            item.Fallar(request.ErrorMessage);
        }
        else
        {
            item.CambiarEstado(nuevoEstado);
        }

        await _repo.UpdateAsync(item);

        _logger.LogInformation("Evolution item {Id} movido a {Estado}", id, nuevoEstado);
        return Ok();
    }

    /// <summary>
    /// Reordena un item dentro de su columna.
    /// </summary>
    [HttpPut("items/{id}/reorder")]
    public async Task<ActionResult> Reorder(int id, [FromBody] ReorderRequest request)
    {
        await _repo.UpdateOrdenAsync(id, request.Orden);
        return Ok();
    }

    /// <summary>
    /// Archiva un item (soft delete).
    /// </summary>
    [HttpDelete("items/{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null)
            return NotFound();

        await _repo.DeleteAsync(id);
        _logger.LogInformation("Evolution item archivado: {Id}", id);
        return Ok();
    }
}
