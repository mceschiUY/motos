using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Artesanal.Comun;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Catalogo.FichaProducto
{
    /// <summary>
    /// READ-ONLY (zona artesanal, plan §4.5). Dos consultas: la cabecera del producto y la matriz
    /// de variantes con existencias y precio.
    ///
    /// El saldo usa el MISMO cálculo que ExistenciasQuery (doc/modelo-stock.md §5), pero la matriz
    /// arranca de PC_VARIANTES con LEFT JOIN al Kardex. Si arrancara del Kardex, el
    /// "HAVING SUM(Delta) distinto de 0" del original se comería las celdas en cero, que en una
    /// matriz talla × color son justo las que hay que mostrar (agotado).
    /// </summary>
    public class FichaProductoHandler : IRequestHandler<FichaProductoQuery, FichaProductoDto?>
    {
        private const string SqlCabecera = @"
SELECT p.Id, p.Codigo, p.Nombre, p.MarcaId, m.Nombre AS MarcaDisplay,
       p.CategoriaId, c.Nombre AS CategoriaDisplay, p.Genero, p.Temporada, p.Material,
       p.PesoGramos, p.Descripcion, p.FichaTecnica, p.TipoCasco, p.Homologacion,
       p.HomologacionVigente, p.FechaVencHomologacion,
       CAST(ISNULL(p.Destacado, 0) AS BIT) AS Destacado,
       CAST(ISNULL(p.Novedad, 0) AS BIT) AS Novedad,
       p.ImagenPrincipalId,
       @DepositoId AS DepositoId, d.Nombre AS DepositoDisplay
FROM PC_PRODUCTOS p
LEFT JOIN PC_MARCAS m ON m.Id = p.MarcaId
LEFT JOIN PC_CATEGORIAS c ON c.Id = p.CategoriaId
LEFT JOIN PC_DEPOSITOS d ON d.Id = @DepositoId
WHERE p.Id = @ProductoId";

        private const string SqlVariantes = @"
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
),
saldo AS (
    SELECT VarianteId, SUM(Delta) AS Disponible
    FROM mov
    WHERE (@DepositoId IS NULL OR DepositoId = @DepositoId)
    GROUP BY VarianteId
)
SELECT v.Id AS VarianteId, v.Sku,
       v.TallaId, t.Nombre AS TallaDisplay, ISNULL(t.Orden, 0) AS TallaOrden,
       v.ColorId, co.Nombre AS ColorDisplay, co.CodigoHex AS ColorHex,
       v.PrecioLista AS PrecioListaUsd, v.CostoEstandar AS CostoEstandarUsd,
       ISNULL(s.Disponible, 0) AS Disponible, v.Activo
FROM PC_VARIANTES v
LEFT JOIN PC_TALLAS t ON t.Id = v.TallaId
LEFT JOIN PC_COLORES co ON co.Id = v.ColorId
LEFT JOIN saldo s ON s.VarianteId = v.Id
WHERE v.ProductoId = @ProductoId
ORDER BY ISNULL(t.Orden, 0), t.Nombre, co.Nombre, v.Sku";

        private sealed class TotalFila { public int VarianteId { get; set; } public decimal Total { get; set; } }

        private const string SqlComprometido = @"
SELECT l.VarianteId, SUM(l.Cantidad) AS Total
FROM PC_PEDIDO_LINEAS l
JOIN PC_PEDIDOS p ON p.Id = l.PedidoId
JOIN PC_VARIANTES v ON v.Id = l.VarianteId
WHERE v.ProductoId = @ProductoId AND p.Estado IN (N'borrador', N'confirmado', N'preparado')
GROUP BY l.VarianteId";

        private const string SqlVendidas30d = @"
SELECT m.VarianteId, SUM(m.Cantidad) AS Total
FROM PC_MOVIMIENTOS_STOCK m
JOIN PC_VARIANTES v ON v.Id = m.VarianteId
WHERE v.ProductoId = @ProductoId AND m.Tipo = 'salida' AND m.Fecha >= DATEADD(DAY, -30, @Hoy)
GROUP BY m.VarianteId";

        private readonly IQueryService _consultas;

        public FichaProductoHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<FichaProductoDto?> Handle(FichaProductoQuery query, CancellationToken cancellationToken)
        {
            if (query.ProductoId <= 0) throw new ArgumentException("Producto inválido");

            var parametros = new { ProductoId = query.ProductoId, DepositoId = query.DepositoId };
            var cabeceras = await _consultas.ConsultarAsync<FichaProductoDto>(SqlCabecera, parametros);
            var ficha = cabeceras.FirstOrDefault();
            if (ficha == null) return null;   // null => el controller devuelve 404

            ficha.Variantes = await _consultas.ConsultarAsync<FichaProductoVarianteDto>(SqlVariantes, parametros);

            // Revisión de escenas 2026-09-12: umbral configurable, comprometido y cobertura por SKU.
            var umbral = await ParametrosAlertas.UmbralStockBajoAsync(_consultas);
            var comprometido = (await _consultas.ConsultarAsync<TotalFila>(SqlComprometido, new { query.ProductoId })).ToDictionary(t => t.VarianteId, t => t.Total);
            var vendidas = (await _consultas.ConsultarAsync<TotalFila>(SqlVendidas30d, new { query.ProductoId, Hoy = Clock.Current.Today })).ToDictionary(t => t.VarianteId, t => t.Total);
            ficha.UmbralStockBajo = umbral;

            foreach (var v in ficha.Variantes)
            {
                v.MargenUsd = Math.Round(v.PrecioListaUsd - v.CostoEstandarUsd, 2, MidpointRounding.AwayFromZero);
                v.MargenPorcentaje = v.CostoEstandarUsd == 0m
                    ? 0m
                    : Math.Round((v.PrecioListaUsd - v.CostoEstandarUsd) / v.CostoEstandarUsd * 100m, 2, MidpointRounding.AwayFromZero);
                v.Comprometido = comprometido.TryGetValue(v.VarianteId, out var c) ? c : 0m;
                v.Vendidas30d = vendidas.TryGetValue(v.VarianteId, out var vv) ? vv : 0m;
                v.CoberturaDias = v.Vendidas30d > 0m && v.Disponible > 0m ? Math.Round(v.Disponible / (v.Vendidas30d / 30m), 1, MidpointRounding.AwayFromZero) : null;
                if (v.Activo && v.Disponible < umbral) ficha.SkusEnAlerta++;
            }

            var activas = ficha.Variantes.Where(v => v.Activo).ToList();
            ficha.PrecioDesdeUsd = activas.Count == 0 ? 0m : activas.Min(v => v.PrecioListaUsd);
            ficha.UnidadesTotales = ficha.Variantes.Sum(v => v.Disponible);

            var conCosto = activas.Where(v => v.CostoEstandarUsd > 0m).ToList();
            ficha.MargenPromedioPorcentaje = conCosto.Count == 0
                ? 0m
                : Math.Round(conCosto.Average(v => v.MargenPorcentaje), 2, MidpointRounding.AwayFromZero);

            return ficha;
        }
    }
}
