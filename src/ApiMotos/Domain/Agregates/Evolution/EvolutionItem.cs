using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Evolution;

public enum EvolutionItemEstado
{
    Backlog = 0,
    Approved = 1,
    Building = 2,
    Review = 3,
    Deployed = 4,
    Rejected = 5,
    Failed = 6,
    Archived = 7
}

/// <summary>
/// Representa un item de evolución en el board Kanban.
/// Puede originarse desde una propuesta del Evolution Hub o crearse manualmente.
/// </summary>
public class EvolutionItem : BaseEntity<int>
{
    public string Titulo { get; private set; } = string.Empty;
    public string Descripcion { get; private set; } = string.Empty;
    public string BusinessValue { get; private set; } = string.Empty;
    public EvolutionItemEstado Estado { get; private set; } = EvolutionItemEstado.Backlog;
    public string Prioridad { get; private set; } = "medium";
    public string? EntidadRelacionada { get; private set; }
    public string Categoria { get; private set; } = string.Empty;
    public string PromptMutation { get; private set; } = string.Empty;
    public string? FeatureId { get; private set; }
    public int? MutacionId { get; private set; }
    public string Icon { get; private set; } = "extension";
    public int Orden { get; private set; } = 0;
    public DateTime FechaCreacion { get; private set; } = DateTime.UtcNow;
    public DateTime? FechaAprobacion { get; private set; }
    public DateTime? FechaInicioBuild { get; private set; }
    public DateTime? FechaDespliegue { get; private set; }
    public string? ErrorMessage { get; private set; }

    // EF constructor
    protected EvolutionItem() { }

    /// <summary>
    /// Factory method para crear un EvolutionItem desde una propuesta del Hub.
    /// </summary>
    public static EvolutionItem Crear(
        string titulo,
        string descripcion,
        string businessValue,
        string prioridad,
        string? entidadRelacionada,
        string categoria,
        string promptMutation,
        string? featureId,
        string icon)
    {
        return new EvolutionItem
        {
            Titulo = titulo,
            Descripcion = descripcion,
            BusinessValue = businessValue,
            Prioridad = prioridad,
            EntidadRelacionada = entidadRelacionada,
            Categoria = categoria,
            PromptMutation = promptMutation,
            FeatureId = featureId,
            Icon = icon,
            Estado = EvolutionItemEstado.Backlog,
            FechaCreacion = DateTime.UtcNow
        };
    }

    public void Aprobar()
    {
        Estado = EvolutionItemEstado.Approved;
        FechaAprobacion = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void IniciarBuild(int? mutacionId = null)
    {
        Estado = EvolutionItemEstado.Building;
        FechaInicioBuild = DateTime.UtcNow;
        MutacionId = mutacionId;
        ErrorMessage = null;
    }

    public void Revisar()
    {
        Estado = EvolutionItemEstado.Review;
    }

    public void Desplegar()
    {
        Estado = EvolutionItemEstado.Deployed;
        FechaDespliegue = DateTime.UtcNow;
    }

    public void Rechazar()
    {
        Estado = EvolutionItemEstado.Rejected;
    }

    public void Fallar(string error)
    {
        Estado = EvolutionItemEstado.Failed;
        ErrorMessage = error;
    }

    public void Archivar()
    {
        Estado = EvolutionItemEstado.Archived;
    }

    public void VolverABacklog()
    {
        Estado = EvolutionItemEstado.Backlog;
        ErrorMessage = null;
        MutacionId = null;
    }

    public void ActualizarOrden(int orden)
    {
        Orden = orden;
    }

    public void CambiarEstado(EvolutionItemEstado nuevoEstado)
    {
        switch (nuevoEstado)
        {
            case EvolutionItemEstado.Approved: Aprobar(); break;
            case EvolutionItemEstado.Building: IniciarBuild(); break;
            case EvolutionItemEstado.Review: Revisar(); break;
            case EvolutionItemEstado.Deployed: Desplegar(); break;
            case EvolutionItemEstado.Rejected: Rechazar(); break;
            case EvolutionItemEstado.Backlog: VolverABacklog(); break;
            case EvolutionItemEstado.Archived: Archivar(); break;
            default: Estado = nuevoEstado; break;
        }
    }
}
