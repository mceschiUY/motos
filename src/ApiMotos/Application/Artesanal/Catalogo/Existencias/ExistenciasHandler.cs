using MediatR;
using ApiMotos.Application.Artesanal.Comun;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Catalogo.Existencias
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Cinco consultas y la composición en memoria: depósitos,
    /// variantes con su producto/marca/categoría (y la categoría raíz por CTE recursiva), saldo
    /// por variante y depósito con la MISMA regla del Kardex que ExistenciasQuery y FichaProducto
    /// (entrada +, salida −, ajuste con signo, transferencia − origen + destino), comprometido por
    /// pedidos abiertos y salidas de los últimos 30 días. Las variantes sin movimientos también
    /// salen (saldo 0): en una pantalla de "qué se está acabando" el cero es el dato.
    /// </summary>
    public class ExistenciasHandler : IRequestHandler<ExistenciasQuery, ExistenciasDto>
    {
        private sealed class VarianteFila
        {
            public int VarianteId { get; set; }
            public string? Sku { get; set; }
            public int ProductoId { get; set; }
            public string? ProductoNombre { get; set; }
            public string? ProductoCodigo { get; set; }
            public string? Marca { get; set; }
            public int? CategoriaId { get; set; }
            public string? Categoria { get; set; }
            public string? CategoriaRaiz { get; set; }
            public string? Talla { get; set; }
            public int TallaOrden { get; set; }
            public string? Color { get; set; }
            public string? ColorHex { get; set; }
            public decimal PrecioListaUsd { get; set; }
        }

        private sealed class SaldoFila
        {
            public int VarianteId { get; set; }
            public int DepositoId { get; set; }
            public string? Nombre { get; set; }
            public decimal Saldo { get; set; }
        }

        private sealed class TotalFila
        {
            public int VarianteId { get; set; }
            public decimal Total { get; set; }
        }

        private const string SqlDepositos = @"
SELECT Id, Nombre FROM PC_DEPOSITOS WHERE Activo = 1 ORDER BY Nombre";

        private const string SqlVariantes = @"
WITH cat AS (
    SELECT Id, Nombre, CategoriaPadreId, Nombre AS RaizNombre, 0 AS Nivel
    FROM PC_CATEGORIAS WHERE CategoriaPadreId IS NULL
    UNION ALL
    SELECT c.Id, c.Nombre, c.CategoriaPadreId, cat.RaizNombre, cat.Nivel + 1
    FROM PC_CATEGORIAS c JOIN cat ON c.CategoriaPadreId = cat.Id
    WHERE cat.Nivel < 5
)
SELECT v.Id AS VarianteId, v.Sku, p.Id AS ProductoId, p.Nombre AS ProductoNombre, p.Codigo AS ProductoCodigo,
       m.Nombre AS Marca, p.CategoriaId, c.Nombre AS Categoria, ISNULL(cat.RaizNombre, c.Nombre) AS CategoriaRaiz,
       t.Nombre AS Talla, ISNULL(t.Orden, 0) AS TallaOrden, co.Nombre AS Color, co.CodigoHex AS ColorHex,
       v.PrecioLista AS PrecioListaUsd
FROM PC_VARIANTES v
JOIN PC_PRODUCTOS p ON p.Id = v.ProductoId
LEFT JOIN PC_MARCAS m ON m.Id = p.MarcaId
LEFT JOIN PC_CATEGORIAS c ON c.Id = p.CategoriaId
LEFT JOIN cat ON cat.Id = c.Id
LEFT JOIN PC_TALLAS t ON t.Id = v.TallaId
LEFT JOIN PC_COLORES co ON co.Id = v.ColorId
WHERE v.Activo = 1 AND p.Activo = 1
ORDER BY ISNULL(cat.RaizNombre, c.Nombre), p.Nombre, ISNULL(t.Orden, 0), t.Nombre, co.Nombre";

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
SELECT mov.VarianteId, mov.DepositoId, d.Nombre, SUM(mov.Delta) AS Saldo
FROM mov
LEFT JOIN PC_DEPOSITOS d ON d.Id = mov.DepositoId
GROUP BY mov.VarianteId, mov.DepositoId, d.Nombre";

        private const string SqlComprometido = @"
SELECT l.VarianteId, SUM(l.Cantidad) AS Total
FROM PC_PEDIDO_LINEAS l
JOIN PC_PEDIDOS p ON p.Id = l.PedidoId
WHERE p.Estado IN (N'borrador', N'confirmado', N'preparado')
GROUP BY l.VarianteId";

        private const string SqlVendidas30d = @"
SELECT VarianteId, SUM(Cantidad) AS Total
FROM PC_MOVIMIENTOS_STOCK
WHERE Tipo = 'salida' AND Fecha >= DATEADD(DAY, -30, @Hoy)
GROUP BY VarianteId";

        private readonly IQueryService _consultas;

        public ExistenciasHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<ExistenciasDto> Handle(ExistenciasQuery query, CancellationToken cancellationToken)
        {
            var hoy = Clock.Current.Today;
            var umbral = await ParametrosAlertas.UmbralStockBajoAsync(_consultas);
            var depositos = await _consultas.ConsultarAsync<ExistenciasDepositoDto>(SqlDepositos);
            var variantes = await _consultas.ConsultarAsync<VarianteFila>(SqlVariantes);
            var saldos = (await _consultas.ConsultarAsync<SaldoFila>(SqlSaldos)).ToLookup(s => s.VarianteId);
            var comprometido = (await _consultas.ConsultarAsync<TotalFila>(SqlComprometido)).ToDictionary(t => t.VarianteId, t => t.Total);
            var vendidas = (await _consultas.ConsultarAsync<TotalFila>(SqlVendidas30d, new { Hoy = hoy })).ToDictionary(t => t.VarianteId, t => t.Total);

            var dto = new ExistenciasDto { UmbralStockBajo = umbral, DepositoId = query.DepositoId, Depositos = depositos };

            foreach (var v in variantes)
            {
                var porDeposito = saldos[v.VarianteId]
                    .Select(s => new ExistenciaSaldoDepositoDto { DepositoId = s.DepositoId, Nombre = s.Nombre, Saldo = s.Saldo })
                    .OrderBy(s => s.Nombre)
                    .ToList();
                var saldo = query.DepositoId is int dep
                    ? porDeposito.Where(s => s.DepositoId == dep).Sum(s => s.Saldo)
                    : porDeposito.Sum(s => s.Saldo);
                var vend = vendidas.TryGetValue(v.VarianteId, out var vv) ? vv : 0m;
                decimal? cobertura = vend > 0m && saldo > 0m
                    ? Math.Round(saldo / (vend / 30m), 1, MidpointRounding.AwayFromZero)
                    : null;

                dto.Filas.Add(new ExistenciaFilaDto
                {
                    VarianteId = v.VarianteId,
                    Sku = v.Sku,
                    ProductoId = v.ProductoId,
                    ProductoNombre = v.ProductoNombre,
                    ProductoCodigo = v.ProductoCodigo,
                    Marca = v.Marca,
                    CategoriaId = v.CategoriaId,
                    Categoria = v.Categoria,
                    CategoriaRaiz = v.CategoriaRaiz,
                    Talla = v.Talla,
                    Color = v.Color,
                    ColorHex = v.ColorHex,
                    PrecioListaUsd = v.PrecioListaUsd,
                    Saldo = saldo,
                    PorDeposito = porDeposito,
                    Comprometido = comprometido.TryGetValue(v.VarianteId, out var c) ? c : 0m,
                    Vendidas30d = vend,
                    CoberturaDias = cobertura,
                    Semaforo = saldo < 0m ? "negativo" : saldo == 0m ? "sin_stock" : saldo < umbral ? "bajo" : "ok",
                });
            }

            return dto;
        }
    }
}
