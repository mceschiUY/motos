using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Pedidos.Queries.Resumen
{
    public class PedidosResumenHandler : GenericResumenHandler<PedidosResumenQuery, PedidosResumenDto, GrupoConteoPedido>
    {
        public PedidosResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_PEDIDOS",
                "SELECT ISNULL(CAST(Estado AS nvarchar(100)), '') AS Clave, COUNT(*) AS Cantidad FROM PC_PEDIDOS GROUP BY Estado",
                @"SELECT FORMAT(Fecha, 'yyyy-MM') AS Clave, COUNT(*) AS Cantidad FROM PC_PEDIDOS
                   WHERE Fecha >= DATEADD(month, -11, GETDATE())
                   GROUP BY FORMAT(Fecha, 'yyyy-MM') ORDER BY Clave",
                (total, porEstado, porMes) => new PedidosResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
