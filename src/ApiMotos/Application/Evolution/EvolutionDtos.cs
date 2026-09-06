using ApiMotos.Domain.Agregates.Evolution;

namespace ApiMotos.Application.Evolution;

// ═══════════════════════════════════════════════════════════════════════════════
// DTOs para Evolution Board
// ═══════════════════════════════════════════════════════════════════════════════

public record EvolutionBoardDto
{
    public List<EvolutionItemDto> Backlog { get; init; } = new();
    public List<EvolutionItemDto> Approved { get; init; } = new();
    public List<EvolutionItemDto> Building { get; init; } = new();
    public List<EvolutionItemDto> Review { get; init; } = new();
    public List<EvolutionItemDto> Deployed { get; init; } = new();
    public List<EvolutionItemDto> Failed { get; init; } = new();
    public List<EvolutionItemDto> Rejected { get; init; } = new();
}

public record EvolutionItemDto
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string BusinessValue { get; init; } = string.Empty;
    public string Estado { get; init; } = string.Empty;
    public string Prioridad { get; init; } = "medium";
    public string? EntidadRelacionada { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public string PromptMutation { get; init; } = string.Empty;
    public string? FeatureId { get; init; }
    public int? MutacionId { get; init; }
    public string Icon { get; init; } = string.Empty;
    public int Orden { get; init; }
    public DateTime FechaCreacion { get; init; }
    public DateTime? FechaAprobacion { get; init; }
    public DateTime? FechaInicioBuild { get; init; }
    public DateTime? FechaDespliegue { get; init; }
    public string? ErrorMessage { get; init; }

    public EvolutionItemDto() { }

    public EvolutionItemDto(EvolutionItem item)
    {
        Id = item.Id;
        Titulo = item.Titulo;
        Descripcion = item.Descripcion;
        BusinessValue = item.BusinessValue;
        Estado = item.Estado.ToString();
        Prioridad = item.Prioridad;
        EntidadRelacionada = item.EntidadRelacionada;
        Categoria = item.Categoria;
        PromptMutation = item.PromptMutation;
        FeatureId = item.FeatureId;
        MutacionId = item.MutacionId;
        Icon = item.Icon;
        Orden = item.Orden;
        FechaCreacion = item.FechaCreacion;
        FechaAprobacion = item.FechaAprobacion;
        FechaInicioBuild = item.FechaInicioBuild;
        FechaDespliegue = item.FechaDespliegue;
        ErrorMessage = item.ErrorMessage;
    }
}

public record CreateEvolutionItemRequest
{
    public string Titulo { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public string BusinessValue { get; init; } = string.Empty;
    public string Prioridad { get; init; } = "medium";
    public string? EntidadRelacionada { get; init; }
    public string Categoria { get; init; } = string.Empty;
    public string PromptMutation { get; init; } = string.Empty;
    public string? FeatureId { get; init; }
    public string Icon { get; init; } = "extension";
}

public record UpdateEstadoRequest
{
    public string Estado { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
}

public record ReorderRequest
{
    public int Orden { get; init; }
}

public record ImportProposalsRequest
{
    public List<ImportProposalItem> Proposals { get; init; } = new();
}

public record ImportProposalItem
{
    public string FeatureId { get; init; } = string.Empty;
    public string FeatureName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string BusinessValue { get; init; } = string.Empty;
    public string Priority { get; init; } = "medium";
    public string? EntityName { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public string Icon { get; init; } = "extension";
}
