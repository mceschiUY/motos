using System.Globalization;
using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Artesanal.Comun;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Control.CentroControl
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Arma el home "Hoy" con varias consultas chicas contra PC_*
    /// (SQL crudo vía IQueryService, como Agenda/Comisiones) y construye el FEED en memoria:
    /// cada acción dice qué pasa y a dónde ir, ordenada por urgencia y con cupo por tipo para
    /// que ningún tipo se coma el feed entero.
    ///
    /// Reglas que copia de otras escenas (no las reinventa):
    /// - Kardex (doc/modelo-stock.md §5): entrada +, salida −, ajuste con signo, transferencia
    ///   − en origen y + en destino. Igual que ExistenciasQuery / FichaProducto.
    /// - SLA (misma que la escena de seguimiento): recibido → etapa `facturacion` desde
    ///   FechaRecibido; facturado → `despacho` desde FechaFactura; despachado → `entrega` desde
    ///   FechaEnvio. dias = hoy − inicio; ok si ≤ umbral, advertencia si ≤ límite, vencido si &gt; límite.
    /// - Visitas de hoy: la misma unión que AgendaHandler (ProximaAccion = hoy o Fecha = hoy).
    /// - Comisiones: la sellada en cada pedido entregado del mes (ComisionesHandler).
    /// </summary>
    public class CentroControlHandler : IRequestHandler<CentroControlQuery, CentroControlDto>
    {
        private const int MaxAcciones = 12;

        // ── Filas internas (una clase por consulta) ─────────────────────────────


        private sealed class KpiFila
        {
            public int PedidosNuevosHoy { get; set; }
            public int PedidosADespachar { get; set; }
            public int VisitasHoy { get; set; }
            public decimal VentasMesUsd { get; set; }
            public decimal VentasMesAnteriorUsd { get; set; }
            public decimal ComisionesMesUsd { get; set; }
        }

        private sealed class EnvioFila
        {
            public int Id { get; set; }
            public string? CodigoRastreo { get; set; }
            public string? Estado { get; set; }
            public string? ClienteDisplay { get; set; }
            public string? Ciudad { get; set; }
            public string? AgenciaDisplay { get; set; }
            public string? Etapa { get; set; }
            public DateTime? Inicio { get; set; }
            public int? Umbral { get; set; }
            public int? Limite { get; set; }
        }

        private sealed class PedidoFila
        {
            public int Id { get; set; }
            public string? Numero { get; set; }
            public string? Estado { get; set; }
            public DateTime Fecha { get; set; }
            public decimal TotalUsd { get; set; }
            public string? ClienteDisplay { get; set; }
            public string? Ciudad { get; set; }
        }

        private sealed class VisitaVendedorFila
        {
            public int VendedorId { get; set; }
            public string? VendedorDisplay { get; set; }
            public int Clientes { get; set; }
            public int Actividades { get; set; }
        }

        private sealed class StockSkuFila
        {
            public int VarianteId { get; set; }
            public string? Sku { get; set; }
            public string? ProductoDisplay { get; set; }
            public string? MarcaDisplay { get; set; }
            public string? TallaDisplay { get; set; }
            public string? ColorDisplay { get; set; }
            public decimal Saldo { get; set; }
        }

        private sealed class SinVisitaFila
        {
            public int ClienteId { get; set; }
            public string? ClienteDisplay { get; set; }
            public string? Ciudad { get; set; }
            public string? VendedorDisplay { get; set; }
            public DateTime? UltimaActividad { get; set; }
            public int? DiasSinVisita { get; set; }
        }

        private sealed class VendedorFila
        {
            public int Id { get; set; }
            public string? Nombre { get; set; }
            public string? Zona { get; set; }
            public decimal VendidoMesUsd { get; set; }
            public decimal ObjetivoUsd { get; set; }
            public decimal ComisionMesUsd { get; set; }
            public int VisitasMes { get; set; }
        }

        // ── SQL ──────────────────────────────────────────────────────────────────

        private const string SqlKpis = @"
SELECT
    (SELECT COUNT(*) FROM PC_PEDIDOS WHERE CAST(Fecha AS date) = @Hoy AND Estado <> N'anulado') AS PedidosNuevosHoy,
    (SELECT COUNT(*) FROM PC_PEDIDOS WHERE Estado IN (N'confirmado', N'preparado')) AS PedidosADespachar,
    (SELECT COUNT(*) FROM PC_ACTIVIDADES WHERE ProximaAccion = @Hoy OR CAST(Fecha AS date) = @Hoy) AS VisitasHoy,
    ISNULL((SELECT SUM(TotalUsd) FROM PC_PEDIDOS
            WHERE Fecha >= @DesdeMes AND Fecha < @HastaMes AND Estado NOT IN (N'borrador', N'anulado')), 0) AS VentasMesUsd,
    ISNULL((SELECT SUM(TotalUsd) FROM PC_PEDIDOS
            WHERE Fecha >= @DesdeMesAnterior AND Fecha < @DesdeMes AND Estado NOT IN (N'borrador', N'anulado')), 0) AS VentasMesAnteriorUsd,
    ISNULL((SELECT SUM(ComisionUsd) FROM PC_PEDIDOS
            WHERE Estado = N'entregado' AND Fecha >= @DesdeMes AND Fecha < @HastaMes), 0) AS ComisionesMesUsd";

        private const string SqlEnvios = @"
SELECT e.Id, e.CodigoRastreo, e.Estado, c.Nombre AS ClienteDisplay, c.Ciudad, a.Nombre AS AgenciaDisplay
     , et.Etapa
     , CASE e.Estado WHEN N'recibido' THEN e.FechaRecibido
                     WHEN N'facturado' THEN ISNULL(e.FechaFactura, e.FechaRecibido)
                     WHEN N'despachado' THEN ISNULL(e.FechaEnvio, ISNULL(e.FechaFactura, e.FechaRecibido)) END AS Inicio
     , s.RangoAlertaUmbralAdvertenciaDias AS Umbral
     , s.RangoAlertaLimiteDias AS Limite
FROM PC_ENVIOS e
CROSS APPLY (SELECT CASE e.Estado WHEN N'recibido' THEN N'facturacion'
                                  WHEN N'facturado' THEN N'despacho'
                                  WHEN N'despachado' THEN N'entrega' END AS Etapa) et
LEFT JOIN PC_PARAMETROSLAS s ON s.Etapa = et.Etapa
LEFT JOIN PC_CLIENTES c ON c.Id = e.ClienteId
LEFT JOIN PC_AGENCIAS a ON a.Id = e.AgenciaId
WHERE e.Estado IN (N'recibido', N'facturado', N'despachado')";

        private const string SqlPedidosADespachar = @"
SELECT p.Id, p.Numero, p.Estado, p.Fecha, p.TotalUsd, c.Nombre AS ClienteDisplay, c.Ciudad
FROM PC_PEDIDOS p
LEFT JOIN PC_CLIENTES c ON c.Id = p.ClienteId
WHERE p.Estado IN (N'confirmado', N'preparado')
ORDER BY CASE p.Estado WHEN N'preparado' THEN 0 ELSE 1 END, p.Fecha, p.Id";

        private const string SqlPedidosNuevosHoy = @"
SELECT p.Id, p.Numero, p.Estado, p.Fecha, p.TotalUsd, c.Nombre AS ClienteDisplay, c.Ciudad
FROM PC_PEDIDOS p
LEFT JOIN PC_CLIENTES c ON c.Id = p.ClienteId
WHERE CAST(p.Fecha AS date) = @Hoy AND p.Estado = N'borrador'
ORDER BY p.Fecha DESC, p.Id DESC";

        private const string SqlVisitasHoy = @"
SELECT v.Id AS VendedorId, v.Nombre AS VendedorDisplay
     , COUNT(DISTINCT a.ClienteId) AS Clientes, COUNT(*) AS Actividades
FROM PC_ACTIVIDADES a
JOIN PC_VENDEDORES v ON v.Id = a.VendedorId
WHERE a.ProximaAccion = @Hoy OR CAST(a.Fecha AS date) = @Hoy
GROUP BY v.Id, v.Nombre
ORDER BY Clientes DESC, v.Nombre";

        private const string KardexCte = @"
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
)";

        private const string SqlStockSku = KardexCte + @",
saldo AS (SELECT VarianteId, SUM(Delta) AS Saldo FROM mov GROUP BY VarianteId)
SELECT v.Id AS VarianteId, v.Sku, p.Nombre AS ProductoDisplay, m.Nombre AS MarcaDisplay
     , t.Nombre AS TallaDisplay, co.Nombre AS ColorDisplay, s.Saldo
FROM saldo s
JOIN PC_VARIANTES v ON v.Id = s.VarianteId
LEFT JOIN PC_PRODUCTOS p ON p.Id = v.ProductoId
LEFT JOIN PC_MARCAS m ON m.Id = p.MarcaId
LEFT JOIN PC_TALLAS t ON t.Id = v.TallaId
LEFT JOIN PC_COLORES co ON co.Id = v.ColorId
WHERE v.Activo = 1 AND s.Saldo < @Umbral AND s.Saldo <> 0
ORDER BY s.Saldo, v.Sku";

        private const string SqlStockDeposito = KardexCte + @"
SELECT d.Id AS DepositoId, d.Nombre, ISNULL(SUM(mov.Delta), 0) AS Unidades
FROM PC_DEPOSITOS d
LEFT JOIN mov ON mov.DepositoId = d.Id
WHERE d.Activo = 1
GROUP BY d.Id, d.Nombre
ORDER BY Unidades DESC, d.Nombre";

        private const string SqlSinVisita = @"
SELECT TOP 5 c.Id AS ClienteId, c.Nombre AS ClienteDisplay, c.Ciudad, v.Nombre AS VendedorDisplay
     , ua.Ultima AS UltimaActividad
     , CASE WHEN ua.Ultima IS NULL THEN NULL ELSE DATEDIFF(DAY, ua.Ultima, @Hoy) END AS DiasSinVisita
FROM PC_CLIENTES c
JOIN PC_VENDEDORES v ON v.Id = c.VendedorId
OUTER APPLY (SELECT MAX(a.Fecha) AS Ultima FROM PC_ACTIVIDADES a WHERE a.ClienteId = c.Id) ua
WHERE c.VendedorId IS NOT NULL
  AND (ua.Ultima IS NULL OR ua.Ultima < DATEADD(DAY, -@Dias, @Hoy))
ORDER BY CASE WHEN ua.Ultima IS NULL THEN 0 ELSE 1 END, ua.Ultima, c.Nombre";

        private const string SqlPipeline = @"
SELECT Estado, COUNT(*) AS Cantidad, ISNULL(SUM(TotalUsd), 0) AS TotalUsd
FROM PC_PEDIDOS
WHERE Fecha >= @DesdeMes AND Fecha < @HastaMes
GROUP BY Estado";

        private const string SqlVendedores = @"
SELECT v.Id, v.Nombre, v.Zona
     , ISNULL((SELECT SUM(p.TotalUsd) FROM PC_PEDIDOS p
               WHERE p.VendedorId = v.Id AND p.Fecha >= @DesdeMes AND p.Fecha < @HastaMes
                 AND p.Estado = N'entregado'), 0) AS VendidoMesUsd -- entregados: misma base que /avance y comisiones
     , ISNULL((SELECT TOP 1 m.ObjetivoUsd FROM PC_METAS m WHERE m.VendedorId = v.Id AND m.Periodo = @Periodo), 0) AS ObjetivoUsd
     , ISNULL((SELECT SUM(p.ComisionUsd) FROM PC_PEDIDOS p
               WHERE p.VendedorId = v.Id AND p.Estado = N'entregado'
                 AND p.Fecha >= @DesdeMes AND p.Fecha < @HastaMes), 0) AS ComisionMesUsd
     , (SELECT COUNT(*) FROM PC_ACTIVIDADES a
        WHERE a.VendedorId = v.Id AND a.Tipo = N'visita' AND a.Fecha >= @DesdeMes AND a.Fecha < @HastaMes) AS VisitasMes
FROM PC_VENDEDORES v
WHERE v.Activo = 1
ORDER BY VendidoMesUsd DESC, v.Nombre";

        private const string SqlTopProductos = @"
SELECT TOP 5 p.Id AS ProductoId, p.Nombre, m.Nombre AS Marca
     , SUM(l.Cantidad) AS UnidadesMes, SUM(l.SubtotalUsd) AS TotalUsd
FROM PC_PEDIDO_LINEAS l
JOIN PC_PEDIDOS pe ON pe.Id = l.PedidoId
JOIN PC_VARIANTES v ON v.Id = l.VarianteId
JOIN PC_PRODUCTOS p ON p.Id = v.ProductoId
LEFT JOIN PC_MARCAS m ON m.Id = p.MarcaId
WHERE pe.Fecha >= @DesdeMes AND pe.Fecha < @HastaMes AND pe.Estado <> N'anulado'
GROUP BY p.Id, p.Nombre, m.Nombre
ORDER BY TotalUsd DESC, UnidadesMes DESC";

        private static readonly string[] EstadosPipeline = { "borrador", "confirmado", "preparado", "despachado", "entregado" };

        private readonly IQueryService _consultas;

        public CentroControlHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<CentroControlDto> Handle(CentroControlQuery query, CancellationToken cancellationToken)
        {
            var hoy = Clock.Current.Today;
            var desdeMes = new DateTime(hoy.Year, hoy.Month, 1);
            var hastaMes = desdeMes.AddMonths(1);
            var periodo = desdeMes.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var umbral = await ParametrosAlertas.UmbralStockBajoAsync(_consultas);
            var diasSinVisita = await ParametrosAlertas.DiasSinVisitaAsync(_consultas);

            var p = new
            {
                Hoy = hoy,
                DesdeMes = desdeMes,
                HastaMes = hastaMes,
                DesdeMesAnterior = desdeMes.AddMonths(-1),
                Periodo = periodo,
                Umbral = umbral,
                Dias = diasSinVisita,
            };

            var kpi = (await _consultas.ConsultarAsync<KpiFila>(SqlKpis, p)).FirstOrDefault() ?? new KpiFila();
            var envios = await _consultas.ConsultarAsync<EnvioFila>(SqlEnvios, p);
            var aDespachar = await _consultas.ConsultarAsync<PedidoFila>(SqlPedidosADespachar, p);
            var nuevosHoy = await _consultas.ConsultarAsync<PedidoFila>(SqlPedidosNuevosHoy, p);
            var visitas = await _consultas.ConsultarAsync<VisitaVendedorFila>(SqlVisitasHoy, p);
            var stockSku = await _consultas.ConsultarAsync<StockSkuFila>(SqlStockSku, p);
            var sinVisita = await _consultas.ConsultarAsync<SinVisitaFila>(SqlSinVisita, p);
            var pipeline = await _consultas.ConsultarAsync<PipelineEstadoDto>(SqlPipeline, p);
            var vendedores = await _consultas.ConsultarAsync<VendedorFila>(SqlVendedores, p);
            var topProductos = await _consultas.ConsultarAsync<TopProductoDto>(SqlTopProductos, p);
            var stockDeposito = await _consultas.ConsultarAsync<StockDepositoDto>(SqlStockDeposito, p);

            // ── SLA de los envíos en curso ──
            var slaEnvios = envios
                .Select(e => new { Envio = e, Semaforo = SemaforoSla(e, hoy), Dias = DiasSla(e, hoy) })
                .Where(x => x.Semaforo != "ok")
                .OrderByDescending(x => x.Semaforo == "vencido")
                .ThenByDescending(x => x.Dias - (x.Envio.Limite ?? 0))
                .ToList();

            // ── Equipo ──
            var equipo = vendedores.Select(v => new VendedorResumenDto
            {
                Id = v.Id,
                Nombre = v.Nombre ?? $"Vendedor #{v.Id}",
                Zona = v.Zona,
                VendidoMesUsd = v.VendidoMesUsd,
                ObjetivoUsd = v.ObjetivoUsd,
                AvancePorcentaje = v.ObjetivoUsd <= 0m ? 0m : Math.Round(v.VendidoMesUsd / v.ObjetivoUsd * 100m, 1, MidpointRounding.AwayFromZero),
                ComisionMesUsd = v.ComisionMesUsd,
                VisitasMes = v.VisitasMes,
            }).ToList();

            var dto = new CentroControlDto
            {
                Fecha = hoy,
                Periodo = periodo,
                UmbralStockBajo = umbral,
                DiasSinVisita = diasSinVisita,
                Kpis = new CentroControlKpisDto
                {
                    PedidosNuevosHoy = kpi.PedidosNuevosHoy,
                    PedidosADespachar = kpi.PedidosADespachar,
                    VisitasHoy = kpi.VisitasHoy,
                    EnviosFueraSla = slaEnvios.Count,
                    EnviosSlaVencidos = slaEnvios.Count(x => x.Semaforo == "vencido"),
                    VentasMesUsd = kpi.VentasMesUsd,
                    VentasMesAnteriorUsd = kpi.VentasMesAnteriorUsd,
                    VariacionMesPorcentaje = kpi.VentasMesAnteriorUsd == 0m
                        ? null
                        : Math.Round((kpi.VentasMesUsd - kpi.VentasMesAnteriorUsd) / kpi.VentasMesAnteriorUsd * 100m, 1, MidpointRounding.AwayFromZero),
                    SkuStockBajo = stockSku.Count(s => s.Saldo > 0m),
                    SkuStockNegativo = stockSku.Count(s => s.Saldo < 0m),
                    ComisionesMesUsd = kpi.ComisionesMesUsd,
                },
                Pipeline = EstadosPipeline.Select(estado =>
                {
                    var fila = pipeline.FirstOrDefault(x => string.Equals(x.Estado, estado, StringComparison.OrdinalIgnoreCase));
                    return new PipelineEstadoDto { Estado = estado, Cantidad = fila?.Cantidad ?? 0, TotalUsd = fila?.TotalUsd ?? 0m };
                }).ToList(),
                Vendedores = equipo,
                TopProductos = topProductos,
                StockPorDeposito = stockDeposito,
            };

            // ── El feed: candidatos con prioridad, cupo por tipo, corte a MaxAcciones ──
            var candidatos = new List<(int Prioridad, AccionDto Accion)>();

            foreach (var x in slaEnvios.Take(4))
            {
                var e = x.Envio;
                var vencido = x.Semaforo == "vencido";
                var destino = string.IsNullOrWhiteSpace(e.Ciudad) ? e.ClienteDisplay : e.Ciudad;
                candidatos.Add((vencido ? 0 : 2, new AccionDto
                {
                    Tipo = "sla",
                    Acento = vencido ? "danger" : "warning",
                    Icono = vencido ? "report" : "schedule",
                    Titulo = $"Envío {e.CodigoRastreo} a {destino} {(vencido ? "fuera de SLA" : "por vencer SLA")}",
                    Detalle = $"{x.Dias} {Dias(x.Dias)} en {EtapaTexto(e.Etapa)} · límite {e.Limite ?? 0} · {e.ClienteDisplay}{(string.IsNullOrWhiteSpace(e.AgenciaDisplay) ? "" : " · " + e.AgenciaDisplay)}",
                    Ruta = $"/envio/{e.Id}",
                    Fecha = e.Inicio,
                }));
            }

            foreach (var s in stockSku.Where(s => s.Saldo < 0m).Take(2))
            {
                candidatos.Add((1, new AccionDto
                {
                    Tipo = "stock",
                    Acento = "danger",
                    Icono = "error",
                    Titulo = $"{NombreSku(s)}: stock en negativo ({Numero(s.Saldo)})",
                    Detalle = $"SKU {s.Sku} · el Kardex quedó por debajo de cero, revisá los movimientos",
                    Ruta = $"/variante/{s.VarianteId}",
                    Fecha = hoy,
                }));
            }

            foreach (var ped in aDespachar.Take(3))
            {
                var preparado = string.Equals(ped.Estado, "preparado", StringComparison.OrdinalIgnoreCase);
                candidatos.Add((preparado ? 3 : 5, new AccionDto
                {
                    Tipo = "despachar",
                    Acento = preparado ? "warning" : "primary",
                    Icono = preparado ? "local_shipping" : "inventory",
                    Titulo = preparado
                        ? $"{ped.Numero} de {ped.ClienteDisplay} listo para despachar"
                        : $"{ped.Numero} de {ped.ClienteDisplay} confirmado, falta preparar",
                    Detalle = $"US$ {Moneda(ped.TotalUsd)} · {ped.Ciudad ?? "sin ciudad"} · hace {DiasDesde(ped.Fecha, hoy)}",
                    Ruta = $"/pedido/{ped.Id}",
                    Fecha = ped.Fecha,
                }));
            }

            foreach (var s in stockSku.Where(s => s.Saldo > 0m).Take(3))
            {
                candidatos.Add((4, new AccionDto
                {
                    Tipo = "stock",
                    Acento = "warning",
                    Icono = "inventory_2",
                    Titulo = $"{NombreSku(s)}: {(s.Saldo == 1m ? "queda 1" : "quedan " + Numero(s.Saldo))}",
                    Detalle = $"SKU {s.Sku} · por debajo del umbral de {Numero(umbral)} · reponer",
                    Ruta = $"/variante/{s.VarianteId}",
                    Fecha = hoy,
                }));
            }

            foreach (var c in sinVisita.Take(2))
            {
                candidatos.Add((6, new AccionDto
                {
                    Tipo = "sin_visita",
                    Acento = "warning",
                    Icono = "person_off",
                    Titulo = c.DiasSinVisita.HasValue
                        ? $"{c.ClienteDisplay} sin visita hace {c.DiasSinVisita} días"
                        : $"{c.ClienteDisplay} nunca fue visitado",
                    Detalle = $"{c.Ciudad ?? "sin ciudad"} · atiende {c.VendedorDisplay} · agendá una visita",
                    Ruta = $"/cliente/{c.ClienteId}",
                    Fecha = c.UltimaActividad,
                }));
            }

            foreach (var v in visitas.Take(2))
            {
                candidatos.Add((7, new AccionDto
                {
                    Tipo = "visita",
                    Acento = "primary",
                    Icono = "route",
                    Titulo = $"{v.VendedorDisplay} visita {v.Clientes} {(v.Clientes == 1 ? "tienda" : "tiendas")} hoy",
                    Detalle = $"{v.Actividades} {(v.Actividades == 1 ? "parada" : "paradas")} en la agenda del día",
                    Ruta = $"/agenda?vendedorId={v.VendedorId}",
                    Fecha = hoy,
                }));
            }

            foreach (var ped in nuevosHoy.Take(2))
            {
                candidatos.Add((8, new AccionDto
                {
                    Tipo = "pedido_nuevo",
                    Acento = "primary",
                    Icono = "receipt_long",
                    Titulo = $"{ped.Numero} de {ped.ClienteDisplay} armado hoy, falta confirmar",
                    Detalle = $"US$ {Moneda(ped.TotalUsd)} · {ped.Ciudad ?? "sin ciudad"}",
                    Ruta = $"/pedido/{ped.Id}",
                    Fecha = ped.Fecha,
                }));
            }

            // Meta: el mejor y el más rezagado (si hay meta cargada).
            var conMeta = equipo.Where(v => v.ObjetivoUsd > 0m).ToList();
            var metas = new List<VendedorResumenDto>();
            if (conMeta.Count > 0) metas.Add(conMeta.OrderByDescending(v => v.AvancePorcentaje).First());
            if (conMeta.Count > 1) metas.Add(conMeta.OrderBy(v => v.AvancePorcentaje).First());
            foreach (var v in metas)
            {
                var cumplida = v.AvancePorcentaje >= 100m;
                var rezagado = !cumplida && v.AvancePorcentaje < RitmoEsperado(hoy) - 20m;
                candidatos.Add((cumplida ? 9 : rezagado ? 6 : 9, new AccionDto
                {
                    Tipo = "meta",
                    Acento = cumplida ? "success" : rezagado ? "warning" : "primary",
                    Icono = cumplida ? "emoji_events" : "flag",
                    Titulo = cumplida
                        ? $"{v.Nombre} superó su meta del mes ({Numero(v.AvancePorcentaje)} %)"
                        : $"{v.Nombre} al {Numero(v.AvancePorcentaje)} % de su meta",
                    Detalle = $"US$ {Moneda(v.VendidoMesUsd)} de US$ {Moneda(v.ObjetivoUsd)}{(rezagado ? " · va por debajo del ritmo del mes" : "")}",
                    Ruta = $"/vendedor/{v.Id}",
                    Fecha = hoy,
                }));
            }

            dto.Acciones = candidatos
                .OrderBy(c => c.Prioridad)
                .Select(c => c.Accion)
                .Take(MaxAcciones)
                .ToList();

            return dto;
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static int DiasSla(EnvioFila e, DateTime hoy)
        {
            var inicio = e.Inicio ?? hoy;
            return Math.Max(0, (hoy - inicio.Date).Days);
        }

        /// <summary>ok | advertencia | vencido. Sin parámetro de SLA para la etapa ⇒ ok (no se puede juzgar).</summary>
        private static string SemaforoSla(EnvioFila e, DateTime hoy)
        {
            if (e.Umbral == null || e.Limite == null) return "ok";
            var dias = DiasSla(e, hoy);
            if (dias <= e.Umbral.Value) return "ok";
            if (dias <= e.Limite.Value) return "advertencia";
            return "vencido";
        }

        /// <summary>Porcentaje del mes transcurrido: lo que "debería" llevar un vendedor para llegar a la meta.</summary>
        private static decimal RitmoEsperado(DateTime hoy)
        {
            var diasMes = DateTime.DaysInMonth(hoy.Year, hoy.Month);
            return Math.Round((decimal)hoy.Day / diasMes * 100m, 1);
        }

        private static string EtapaTexto(string? etapa) => etapa switch
        {
            "facturacion" => "facturación",
            "despacho" => "despacho",
            "entrega" => "entrega",
            _ => "curso",
        };

        private static string NombreSku(StockSkuFila s)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(s.MarcaDisplay)) partes.Add(s.MarcaDisplay);
            if (!string.IsNullOrWhiteSpace(s.ProductoDisplay)) partes.Add(s.ProductoDisplay);
            if (!string.IsNullOrWhiteSpace(s.TallaDisplay)) partes.Add(s.TallaDisplay);
            if (!string.IsNullOrWhiteSpace(s.ColorDisplay)) partes.Add(s.ColorDisplay.ToLowerInvariant());
            return partes.Count == 0 ? (s.Sku ?? $"SKU #{s.VarianteId}") : string.Join(" ", partes);
        }

        private static string Dias(int n) => n == 1 ? "día" : "días";

        private static string DiasDesde(DateTime fecha, DateTime hoy)
        {
            var d = Math.Max(0, (hoy - fecha.Date).Days);
            return d == 0 ? "hoy" : d == 1 ? "1 día" : $"{d} días";
        }

        private static string Numero(decimal n) => n.ToString("0.##", CultureInfo.InvariantCulture);

        private static string Moneda(decimal n) => n.ToString("N0", CultureInfo.GetCultureInfo("es-UY"));
    }
}
