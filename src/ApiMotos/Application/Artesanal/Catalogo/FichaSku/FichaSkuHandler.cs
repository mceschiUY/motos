using MediatR;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Application.Artesanal.Catalogo.FichaSku
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Cinco consultas: cabecera del SKU, saldo por depósito,
    /// comprometido en pedidos abiertos, Kardex completo y pedidos abiertos que lo contienen.
    ///
    /// Regla del Kardex (la MISMA que ExistenciasQuery y FichaProductoHandler, doc/modelo-stock.md §5):
    /// entrada +Cantidad · salida −Cantidad · ajuste Cantidad con signo · transferencia −Cantidad en
    /// DepositoId y +Cantidad en DepositoDestinoId. Sobre el TOTAL una transferencia vale 0, por eso
    /// el saldo corrido se calcula en C# ordenando por fecha e id, sin duplicar filas.
    /// </summary>
    public class FichaSkuHandler : IRequestHandler<FichaSkuQuery, FichaSkuDto?>
    {
        private const string SqlCabecera = @"
SELECT v.Id AS VarianteId, v.Sku, v.CodigoBarras, v.Activo,
       v.ProductoId, p.Nombre AS ProductoNombre, p.Codigo AS ProductoCodigo,
       m.Nombre AS MarcaDisplay, c.Nombre AS CategoriaDisplay, p.ImagenPrincipalId,
       t.Nombre AS TallaDisplay, co.Nombre AS ColorDisplay, co.CodigoHex AS ColorHex,
       v.PrecioLista AS PrecioListaUsd, v.CostoEstandar AS CostoEstandarUsd
FROM PC_VARIANTES v
INNER JOIN PC_PRODUCTOS p ON p.Id = v.ProductoId
LEFT JOIN PC_MARCAS m ON m.Id = p.MarcaId
LEFT JOIN PC_CATEGORIAS c ON c.Id = p.CategoriaId
LEFT JOIN PC_TALLAS t ON t.Id = v.TallaId
LEFT JOIN PC_COLORES co ON co.Id = v.ColorId
WHERE v.Id = @VarianteId";

        private const string SqlStockPorDeposito = @"
WITH mov AS (
    SELECT DepositoId,
        CASE Tipo WHEN 'entrada' THEN Cantidad
                  WHEN 'salida' THEN -Cantidad
                  WHEN 'ajuste' THEN Cantidad
                  WHEN 'transferencia' THEN -Cantidad END AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    WHERE VarianteId = @VarianteId
    UNION ALL
    SELECT DepositoDestinoId AS DepositoId, Cantidad AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    WHERE VarianteId = @VarianteId AND Tipo = 'transferencia' AND DepositoDestinoId IS NOT NULL
)
SELECT mov.DepositoId, d.Nombre AS DepositoNombre, SUM(mov.Delta) AS Saldo
FROM mov
LEFT JOIN PC_DEPOSITOS d ON d.Id = mov.DepositoId
GROUP BY mov.DepositoId, d.Nombre
ORDER BY d.Nombre";

        private const string SqlComprometido = @"
SELECT ISNULL(SUM(l.Cantidad), 0) AS Total
FROM PC_PEDIDO_LINEAS l
INNER JOIN PC_PEDIDOS p ON p.Id = l.PedidoId
WHERE l.VarianteId = @VarianteId AND p.Estado IN ('borrador', 'confirmado', 'preparado')";

        private const string SqlKardex = @"
SELECT m.Id AS MovimientoId, m.Fecha, m.Tipo, m.Cantidad,
       m.DepositoId, d.Nombre AS DepositoNombre,
       m.DepositoDestinoId, dd.Nombre AS DepositoDestinoNombre,
       m.DocumentoOrigen, p.Id AS PedidoId, m.Motivo, m.CostoUnitario, m.Usuario
FROM PC_MOVIMIENTOS_STOCK m
LEFT JOIN PC_DEPOSITOS d ON d.Id = m.DepositoId
LEFT JOIN PC_DEPOSITOS dd ON dd.Id = m.DepositoDestinoId
LEFT JOIN PC_PEDIDOS p ON p.Numero = m.DocumentoOrigen AND p.Numero <> ''
WHERE m.VarianteId = @VarianteId
ORDER BY m.Fecha ASC, m.Id ASC";

        private const string SqlPedidosAbiertos = @"
SELECT p.Id AS PedidoId, p.Numero, p.Estado, c.Nombre AS ClienteNombre,
       SUM(l.Cantidad) AS Cantidad, p.Fecha
FROM PC_PEDIDO_LINEAS l
INNER JOIN PC_PEDIDOS p ON p.Id = l.PedidoId
LEFT JOIN PC_CLIENTES c ON c.Id = p.ClienteId
WHERE l.VarianteId = @VarianteId AND p.Estado NOT IN ('entregado', 'anulado')
GROUP BY p.Id, p.Numero, p.Estado, c.Nombre, p.Fecha
ORDER BY p.Fecha DESC, p.Id DESC";

        private class TotalDto { public decimal Total { get; set; } }

        private readonly IQueryService _consultas;

        public FichaSkuHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<FichaSkuDto?> Handle(FichaSkuQuery query, CancellationToken cancellationToken)
        {
            if (query.VarianteId <= 0) throw new ArgumentException("Variante inválida");

            var parametros = new { query.VarianteId };
            var ficha = (await _consultas.ConsultarAsync<FichaSkuDto>(SqlCabecera, parametros)).FirstOrDefault();
            if (ficha == null) return null;   // null => el controller devuelve 404

            ficha.MargenUsd = Math.Round(ficha.PrecioListaUsd - ficha.CostoEstandarUsd, 2, MidpointRounding.AwayFromZero);
            ficha.MargenPorcentaje = ficha.CostoEstandarUsd == 0m
                ? 0m
                : Math.Round((ficha.PrecioListaUsd - ficha.CostoEstandarUsd) / ficha.CostoEstandarUsd * 100m, 2, MidpointRounding.AwayFromZero);

            ficha.StockPorDeposito = await _consultas.ConsultarAsync<FichaSkuDepositoDto>(SqlStockPorDeposito, parametros);
            ficha.StockTotal = ficha.StockPorDeposito.Sum(d => d.Saldo);

            var comprometido = (await _consultas.ConsultarAsync<TotalDto>(SqlComprometido, parametros)).FirstOrDefault();
            ficha.Comprometido = comprometido?.Total ?? 0m;
            ficha.DisponibleNeto = ficha.StockTotal - ficha.Comprometido;

            // Kardex cronológico con saldo corrido sobre el total (todos los depósitos).
            var kardex = await _consultas.ConsultarAsync<FichaSkuMovimientoDto>(SqlKardex, parametros);
            decimal saldo = 0m;
            foreach (var m in kardex)
            {
                m.Delta = (m.Tipo ?? string.Empty).ToLowerInvariant() switch
                {
                    "entrada" => m.Cantidad,
                    "salida" => -m.Cantidad,
                    "ajuste" => m.Cantidad,
                    // Con destino, el total no cambia; sin destino (dato viejo) es una salida.
                    "transferencia" => m.DepositoDestinoId.HasValue ? 0m : -m.Cantidad,
                    _ => 0m,
                };
                saldo += m.Delta;
                m.SaldoAcumulado = saldo;
            }
            kardex.Reverse();
            ficha.Kardex = kardex;

            var desde = DateTime.UtcNow.AddDays(-30);
            ficha.UnidadesVendidas30d = kardex
                .Where(m => string.Equals(m.Tipo, "salida", StringComparison.OrdinalIgnoreCase) && m.Fecha >= desde)
                .Sum(m => m.Cantidad);
            ficha.CoberturaDias = ficha.UnidadesVendidas30d <= 0m
                ? null
                : Math.Round(ficha.StockTotal / (ficha.UnidadesVendidas30d / 30m), 1, MidpointRounding.AwayFromZero);

            ficha.SemaforoStock = ficha.StockTotal < 0m ? "negativo"
                : ficha.StockTotal == 0m ? "sin_stock"
                : ficha.StockTotal < 3m ? "bajo"
                : "ok";

            ficha.PedidosAbiertos = await _consultas.ConsultarAsync<FichaSkuPedidoDto>(SqlPedidosAbiertos, parametros);

            return ficha;
        }
    }
}
