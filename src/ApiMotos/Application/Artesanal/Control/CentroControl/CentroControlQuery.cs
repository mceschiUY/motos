using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Control.CentroControl
{
    /// <summary>
    /// HOY: el centro de control como feed de acciones (home). Una sola llamada con los KPI del
    /// día, las acciones pendientes ordenadas por urgencia, el pipeline de pedidos del mes,
    /// el equipo contra su meta, los productos más vendidos y el stock por depósito.
    /// </summary>
    public record CentroControlQuery() : IQuery<CentroControlDto>;
}
