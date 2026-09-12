using MediatR;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Application.Artesanal.Comercial.FichaPedido
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Tres consultas: cabecera (con cliente, vendedor, depósito,
    /// agencia y el envío vinculado por PC_PEDIDOS.EnvioId o PC_ENVIOS.PedidoId), líneas con su
    /// variante/producto/talla/color y foto, y el saldo actual de cada variante en el depósito
    /// del pedido (misma regla del Kardex que el resto de las escenas). El faltante solo tiene
    /// sentido mientras el pedido no salió del depósito (borrador/confirmado/preparado).
    /// </summary>
    public class FichaPedidoHandler : IRequestHandler<FichaPedidoQuery, FichaPedidoDto?>
    {
        private sealed class SaldoFila { public int VarianteId { get; set; } public decimal Saldo { get; set; } }

        private const string SqlCabecera = @"
SELECT p.Id, p.Numero, p.Fecha, p.Estado, p.TotalUsd, p.ComisionUsd, ISNULL(v.ComisionPorcentaje, 0) AS ComisionPorcentaje,
       p.Observaciones, p.MotivoAnulacion,
       p.ClienteId, c.Nombre AS ClienteNombre, c.Tipo AS ClienteTipo, c.Ciudad AS ClienteCiudad,
       c.DireccionEntrega AS ClienteDireccion, c.Telefono AS ClienteTelefono,
       p.VendedorId, v.Nombre AS VendedorNombre,
       p.DepositoId, d.Nombre AS DepositoNombre,
       p.AgenciaId, a.Nombre AS AgenciaNombre,
       e.Id AS EnvioId, e.CodigoRastreo AS EnvioCodigo, e.Estado AS EnvioEstado,
       e.FechaEnvio AS EnvioFechaEnvio, e.FechaEntrega AS EnvioFechaEntrega
FROM PC_PEDIDOS p
LEFT JOIN PC_CLIENTES c ON c.Id = p.ClienteId
LEFT JOIN PC_VENDEDORES v ON v.Id = p.VendedorId
LEFT JOIN PC_DEPOSITOS d ON d.Id = p.DepositoId
LEFT JOIN PC_AGENCIAS a ON a.Id = p.AgenciaId
OUTER APPLY (
    SELECT TOP 1 x.Id, x.CodigoRastreo, x.Estado, x.FechaEnvio, x.FechaEntrega
    FROM PC_ENVIOS x
    WHERE x.Id = p.EnvioId OR x.PedidoId = p.Id
    ORDER BY CASE WHEN x.Id = p.EnvioId THEN 0 ELSE 1 END, x.Id DESC
) e
WHERE p.Id = @PedidoId";

        private const string SqlLineas = @"
SELECT l.Id, l.VarianteId, va.Sku, pr.Id AS ProductoId, pr.Nombre AS ProductoNombre, m.Nombre AS Marca,
       pr.ImagenPrincipalId, t.Nombre AS Talla, co.Nombre AS Color, co.CodigoHex AS ColorHex,
       l.Cantidad, l.PrecioUnitarioUsd, l.SubtotalUsd, va.PrecioLista AS PrecioListaUsd
FROM PC_PEDIDO_LINEAS l
JOIN PC_VARIANTES va ON va.Id = l.VarianteId
JOIN PC_PRODUCTOS pr ON pr.Id = va.ProductoId
LEFT JOIN PC_MARCAS m ON m.Id = pr.MarcaId
LEFT JOIN PC_TALLAS t ON t.Id = va.TallaId
LEFT JOIN PC_COLORES co ON co.Id = va.ColorId
WHERE l.PedidoId = @PedidoId
ORDER BY pr.Nombre, ISNULL(t.Orden, 0), t.Nombre, co.Nombre, l.Id";

        private const string SqlSaldos = @"
WITH mov AS (
    SELECT VarianteId, DepositoId,
        CASE Tipo WHEN 'entrada' THEN Cantidad
                  WHEN 'salida' THEN -Cantidad
                  WHEN 'ajuste' THEN Cantidad
                  WHEN 'transferencia' THEN -Cantidad END AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    UNION ALL
    SELECT VarianteId, DepositoDestinoId AS DepositoId, Cantidad AS Delta
    FROM PC_MOVIMIENTOS_STOCK
    WHERE Tipo = 'transferencia' AND DepositoDestinoId IS NOT NULL
)
SELECT mov.VarianteId, SUM(mov.Delta) AS Saldo
FROM mov
WHERE mov.DepositoId = @DepositoId
  AND mov.VarianteId IN (SELECT VarianteId FROM PC_PEDIDO_LINEAS WHERE PedidoId = @PedidoId)
GROUP BY mov.VarianteId";

        private static readonly string[] EstadosEnDeposito = { "borrador", "confirmado", "preparado" };

        private readonly IQueryService _consultas;

        public FichaPedidoHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<FichaPedidoDto?> Handle(FichaPedidoQuery query, CancellationToken cancellationToken)
        {
            if (query.PedidoId <= 0) throw new ArgumentException("Pedido inválido");

            var p = new { PedidoId = query.PedidoId };
            var ficha = (await _consultas.ConsultarAsync<FichaPedidoDto>(SqlCabecera, p)).FirstOrDefault();
            if (ficha == null) return null;

            ficha.Lineas = await _consultas.ConsultarAsync<FichaPedidoLineaDto>(SqlLineas, p);
            ficha.Unidades = ficha.Lineas.Sum(l => l.Cantidad);

            if (EstadosEnDeposito.Contains((ficha.Estado ?? "").ToLowerInvariant()))
            {
                var saldos = (await _consultas.ConsultarAsync<SaldoFila>(SqlSaldos, new { query.PedidoId, ficha.DepositoId }))
                    .ToDictionary(s => s.VarianteId, s => s.Saldo);
                foreach (var l in ficha.Lineas)
                {
                    l.DisponibleDeposito = saldos.TryGetValue(l.VarianteId, out var s) ? s : 0m;
                    if (l.Cantidad > l.DisponibleDeposito) ficha.LineasConFaltante++;
                }
            }

            return ficha;
        }
    }
}
