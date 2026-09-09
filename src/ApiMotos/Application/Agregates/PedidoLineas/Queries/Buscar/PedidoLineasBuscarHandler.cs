using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.PedidoLineas.Queries.PedidoLineas;

namespace ApiMotos.Application.Agregates.PedidoLineas.Queries.Buscar
{
    public class PedidoLineasBuscarHandler : GenericBuscarHandler<PedidoLineasBuscarQuery, PedidoLineasDto>
    {
        private const string Sql = @"SELECT TOP 10 e.*
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
                WHERE pedido.Numero LIKE @q OR variante.Sku LIKE @q OR producto.Nombre LIKE @q
                ORDER BY e.Id DESC";

        public PedidoLineasBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
