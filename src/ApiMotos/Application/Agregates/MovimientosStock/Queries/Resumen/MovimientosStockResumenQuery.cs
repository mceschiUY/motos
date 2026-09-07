using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.Resumen
{
    public record MovimientosStockResumenQuery() : IQuery<MovimientosStockResumenDto>;

    public class MovimientosStockResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoMovimientoStock> PorEstado { get; set; } = new();
        public List<GrupoConteoMovimientoStock> PorMes { get; set; } = new();
    }

    public class GrupoConteoMovimientoStock
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
