using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.PedidoLineas.Queries.Resumen
{
    public class PedidoLineasResumenHandler : GenericResumenHandler<PedidoLineasResumenQuery, PedidoLineasResumenDto, GrupoConteoPedidoLinea>
    {
        public PedidoLineasResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_PEDIDO_LINEAS",
                // "Estado" de una línea = el estado del pedido al que pertenece.
                @"SELECT ISNULL(CAST(p.Estado AS nvarchar(100)), '') AS Clave, COUNT(*) AS Cantidad
                   FROM PC_PEDIDO_LINEAS e LEFT JOIN PC_PEDIDOS p ON e.PedidoId = p.Id GROUP BY p.Estado",
                @"SELECT FORMAT(p.Fecha, 'yyyy-MM') AS Clave, COUNT(*) AS Cantidad
                   FROM PC_PEDIDO_LINEAS e JOIN PC_PEDIDOS p ON e.PedidoId = p.Id
                   WHERE p.Fecha >= DATEADD(month, -11, GETDATE())
                   GROUP BY FORMAT(p.Fecha, 'yyyy-MM') ORDER BY Clave",
                (total, porEstado, porMes) => new PedidoLineasResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
