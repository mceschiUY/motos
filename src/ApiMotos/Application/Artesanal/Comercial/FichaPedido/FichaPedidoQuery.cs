using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Comercial.FichaPedido
{
    /// <summary>
    /// Escena "Pedido" (revisión de escenas 2026-09-12, ítem 1): el pedido contado completo para
    /// pisar la ficha generada: cabecera con cliente, vendedor, depósito y agencia; líneas con
    /// producto, foto, talla, color, precio, subtotal y stock del depósito; totales; el envío que
    /// nació al despachar; y el % de comisión del vendedor. READ-ONLY.
    /// </summary>
    public record FichaPedidoQuery(int PedidoId) : IQuery<FichaPedidoDto?>;
}
