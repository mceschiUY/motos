using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.PedidoLineas
{
    /// <summary>
    /// Línea de pedido (plan §3.5): un SKU (Variante) con cantidad y precio. El precio se
    /// COPIA de la variante al momento de armar el pedido — si mañana cambia la lista de
    /// precios, el pedido viejo sigue valiendo lo que valía. El subtotal es derivado.
    /// </summary>
    public class PedidoLinea : BaseEntity<int>
    {
        private PedidoLinea() : base() { }

        private PedidoLinea(int pPedidoId, int pVarianteId, decimal pCantidad, decimal pPrecioUnitarioUsd)
        {
            PedidoId = pPedidoId;
            VarianteId = pVarianteId;
            Cantidad = pCantidad;
            PrecioUnitarioUsd = pPrecioUnitarioUsd;
            SubtotalUsd = Redondear(pCantidad * pPrecioUnitarioUsd);
        }

        public int PedidoId { get; private set; }
        public int VarianteId { get; private set; }
        public decimal Cantidad { get; private set; }
        public decimal PrecioUnitarioUsd { get; private set; }
        public decimal SubtotalUsd { get; private set; }

        private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);

        private static Result Validar(int pedidoId, int varianteId, decimal cantidad, decimal precioUnitarioUsd)
        {
            if (pedidoId <= 0) return Result.Fail("Pedido es requerido");
            if (varianteId <= 0) return Result.Fail("SKU es requerido");
            if (cantidad <= 0) return Result.Fail("La cantidad debe ser mayor que cero");
            if (precioUnitarioUsd < 0) return Result.Fail("El precio no puede ser negativo");
            return Result.Ok();
        }

        public static Result<PedidoLinea> Crear(int pedidoId, int varianteId, decimal cantidad, decimal precioUnitarioUsd)
        {
            var v = Validar(pedidoId, varianteId, cantidad, precioUnitarioUsd);
            if (v.IsFailed) return Result.Fail<PedidoLinea>(v.Errors);
            return new PedidoLinea(pedidoId, varianteId, cantidad, precioUnitarioUsd);
        }

        public Result<PedidoLinea> Modificar(int pPedidoId, int pVarianteId, decimal pCantidad, decimal pPrecioUnitarioUsd)
        {
            var v = Validar(pPedidoId, pVarianteId, pCantidad, pPrecioUnitarioUsd);
            if (v.IsFailed) return Result.Fail<PedidoLinea>(v.Errors);
            PedidoId = pPedidoId;
            VarianteId = pVarianteId;
            Cantidad = pCantidad;
            PrecioUnitarioUsd = pPrecioUnitarioUsd;
            SubtotalUsd = Redondear(pCantidad * pPrecioUnitarioUsd);
            return this;
        }
    }
}
