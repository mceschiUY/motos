using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.PedidoLineas.Queries.PedidoLineas;

namespace ApiMotos.Application.Agregates.PedidoLineas.Queries.ByPedidoId
{
    public class PedidoLineasByPedidoIdHandler : GenericPorFkHandler<PedidoLineasByPedidoIdQuery, PedidoLineasDto>
    {
        private const string Sql = @"
SELECT e.*
    , pedido.Numero AS PedidoDisplay
    , variante.Sku AS VarianteDisplay
    , producto.Nombre AS ProductoDisplay
    , talla.Nombre AS TallaDisplay
    , color.Nombre AS ColorDisplay
FROM PC_PEDIDO_LINEAS e
LEFT JOIN PC_PEDIDOS pedido ON e.PedidoId = pedido.Id
LEFT JOIN PC_VARIANTES variante ON e.VarianteId = variante.Id
LEFT JOIN PC_PRODUCTOS producto ON variante.ProductoId = producto.Id
LEFT JOIN PC_TALLAS talla ON variante.TallaId = talla.Id
LEFT JOIN PC_COLORES color ON variante.ColorId = color.Id
WHERE e.PedidoId = @PedidoId
ORDER BY e.Id";

        public PedidoLineasByPedidoIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { PedidoId = q.PedidoId })
        {
        }
    }
}
