using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.Resumen
{
    public class MovimientosStockResumenHandler : GenericResumenHandler<MovimientosStockResumenQuery, MovimientosStockResumenDto, GrupoConteoMovimientoStock>
    {
        public MovimientosStockResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_MOVIMIENTOS_STOCK",
                "SELECT Tipo AS Clave, COUNT(*) AS Cantidad FROM PC_MOVIMIENTOS_STOCK GROUP BY Tipo",
                null,
                (total, porEstado, porMes) => new MovimientosStockResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
