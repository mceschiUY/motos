using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Logistica.SeguimientoEnvio
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Arma el seguimiento del envío estilo courier: 4 hitos
    /// (recibido → confirmado → despachado → entregado) con el SLA por etapa de PC_PARAMETROSLAS.
    ///
    /// Regla SLA (la misma que usa el home "Hoy"): la etapa PENDIENTE de un envío en `recibido`
    /// es `facturacion` y cuenta desde FechaRecibido; en `facturado` es `despacho` desde
    /// FechaFactura; en `despachado` es `entrega` desde FechaEnvio. dias = hoy − inicio (días
    /// enteros). ok si dias ≤ umbral · advertencia si umbral &lt; dias ≤ límite · vencido si
    /// dias &gt; límite. `entregado` y `anulado` no tienen SLA. Para las etapas ya cumplidas se
    /// informan los días que tardó (hito anterior → hito) y si cumplió el límite.
    ///
    /// Cumplido/pendiente se deduce del ESTADO, no de las fechas: el agregado Envio no tiene
    /// fechas nulas (las etapas no alcanzadas repiten la anterior, ver Seed_Dominio_Motos.sql),
    /// así que una fecha cargada no prueba que el hito se haya alcanzado.
    /// </summary>
    public class SeguimientoEnvioHandler : IRequestHandler<SeguimientoEnvioQuery, SeguimientoEnvioDto?>
    {
        private const string SqlCabecera = @"
SELECT TOP 1
       e.Id AS EnvioId, e.CodigoRastreo, e.Estado, e.MotivoAnulacion,
       e.FechaRecibido, e.FechaFactura, e.FechaEnvio, e.FechaEntrega, e.PedidoId,
       e.ClienteId, c.Nombre AS ClienteNombre, c.Ciudad AS ClienteCiudad,
       c.DireccionEntrega AS ClienteDireccion, c.Telefono AS ClienteTelefono,
       e.AgenciaId, a.Nombre AS AgenciaNombre
FROM PC_ENVIOS e
LEFT JOIN PC_CLIENTES c ON c.Id = e.ClienteId
LEFT JOIN PC_AGENCIAS a ON a.Id = e.AgenciaId
WHERE (@EnvioId IS NOT NULL AND e.Id = @EnvioId)
   OR (@Codigo IS NOT NULL AND UPPER(LTRIM(RTRIM(e.CodigoRastreo))) = UPPER(@Codigo))
ORDER BY e.Id DESC";

        private const string SqlSla = @"
SELECT Etapa, RangoAlertaUmbralAdvertenciaDias AS Umbral, RangoAlertaLimiteDias AS Limite
FROM PC_PARAMETROSLAS";

        // El envío conoce a su pedido (PC_ENVIOS.PedidoId, Etapa B); si el enlace no está,
        // se busca al revés por PC_PEDIDOS.EnvioId. Un solo pedido por envío.
        private const string SqlPedido = @"
SELECT TOP 1 p.Id, p.Numero, p.Fecha, p.Estado, v.Nombre AS VendedorNombre, p.TotalUsd
FROM PC_PEDIDOS p
LEFT JOIN PC_VENDEDORES v ON v.Id = p.VendedorId
WHERE (@PedidoId IS NOT NULL AND p.Id = @PedidoId)
   OR (@PedidoId IS NULL AND p.EnvioId = @EnvioId)
ORDER BY p.Id DESC";

        private const string SqlLineas = @"
SELECT l.VarianteId, va.Sku, pr.Nombre AS ProductoNombre, pr.ImagenPrincipalId, t.Nombre AS Talla, co.Nombre AS Color,
       l.Cantidad, l.PrecioUnitarioUsd, l.SubtotalUsd
FROM PC_PEDIDO_LINEAS l
JOIN PC_VARIANTES va ON va.Id = l.VarianteId
LEFT JOIN PC_PRODUCTOS pr ON pr.Id = va.ProductoId
LEFT JOIN PC_TALLAS t ON t.Id = va.TallaId
LEFT JOIN PC_COLORES co ON co.Id = va.ColorId
WHERE l.PedidoId = @PedidoId
ORDER BY l.Id";

        private const string SqlObservaciones = @"
SELECT Id, FechaHora, Usuario, Texto
FROM PC_OBSERVACIONES
WHERE EnvioId = @EnvioId
ORDER BY FechaHora DESC, Id DESC";

        /// <summary>Orden del ciclo. 'anulado' queda afuera: no es un hito.</summary>
        private static readonly string[] Estados = { "recibido", "facturado", "despachado", "entregado" };
        private static readonly string[] Claves = { "recibido", "confirmado", "despachado", "entregado" };
        private static readonly string[] Labels = { "Recibido", "Confirmado", "Despachado", "Entregado" };
        /// <summary>Etapa SLA que lleva AL hito i (el hito 0 no tiene etapa previa).</summary>
        private static readonly string?[] EtapasSla = { null, "facturacion", "despacho", "entrega" };

        private readonly IQueryService _consultas;
        private readonly IClock _reloj;

        public SeguimientoEnvioHandler(IQueryService consultas, IClock reloj)
        {
            _consultas = consultas;
            _reloj = reloj;
        }

        public async Task<SeguimientoEnvioDto?> Handle(SeguimientoEnvioQuery query, CancellationToken cancellationToken)
        {
            var codigo = string.IsNullOrWhiteSpace(query.CodigoRastreo) ? null : query.CodigoRastreo.Trim();
            if (query.EnvioId is null or <= 0 && codigo == null)
                throw new ArgumentException("Hay que indicar el id del envío o su código de rastreo");

            var cab = (await _consultas.ConsultarAsync<FilaCabecera>(SqlCabecera,
                new { EnvioId = query.EnvioId is > 0 ? query.EnvioId : null, Codigo = codigo })).FirstOrDefault();
            if (cab == null) return null;   // null => 404 en el controller

            var estado = (cab.Estado ?? string.Empty).Trim().ToLowerInvariant();
            var dto = new SeguimientoEnvioDto
            {
                EnvioId = cab.EnvioId,
                CodigoRastreo = cab.CodigoRastreo ?? string.Empty,
                Estado = estado,
                EstadoLabel = LabelEstado(estado),
                MotivoAnulacion = estado == "anulado" && !string.IsNullOrWhiteSpace(cab.MotivoAnulacion) ? cab.MotivoAnulacion : null,
                Publico = query.Publico,
                Cliente = new SeguimientoClienteDto
                {
                    Id = cab.ClienteId,
                    Nombre = cab.ClienteNombre,
                    Ciudad = cab.ClienteCiudad,
                    DireccionEntrega = cab.ClienteDireccion,
                    Telefono = query.Publico ? null : cab.ClienteTelefono,
                },
                Agencia = new SeguimientoAgenciaDto { Id = cab.AgenciaId, Nombre = cab.AgenciaNombre },
            };

            var sla = (await _consultas.ConsultarAsync<FilaSla>(SqlSla))
                .GroupBy(s => (s.Etapa ?? string.Empty).Trim().ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            dto.Etapas = ArmarEtapas(cab, estado, sla, _reloj.Now.Date);
            dto.SlaGlobal = dto.Etapas.FirstOrDefault(e => e.Actual && !e.Cumplida)?.EstadoSla;

            // ── Pedido origen ──
            var pedido = (await _consultas.ConsultarAsync<FilaPedido>(SqlPedido,
                new { PedidoId = cab.PedidoId is > 0 ? cab.PedidoId : null, EnvioId = cab.EnvioId })).FirstOrDefault();
            if (pedido != null)
            {
                var lineas = await _consultas.ConsultarAsync<FilaLinea>(SqlLineas, new { PedidoId = pedido.Id });
                dto.Pedido = new SeguimientoPedidoDto
                {
                    Id = pedido.Id,
                    Numero = pedido.Numero,
                    Fecha = pedido.Fecha,
                    Estado = pedido.Estado,
                    VendedorNombre = pedido.VendedorNombre,
                    TotalUsd = query.Publico ? null : pedido.TotalUsd,
                    Lineas = lineas.Select(l => new SeguimientoLineaDto
                    {
                        VarianteId = l.VarianteId,
                        Sku = l.Sku,
                        ProductoNombre = l.ProductoNombre,
                        ImagenPrincipalId = l.ImagenPrincipalId,
                        Talla = l.Talla,
                        Color = l.Color,
                        Cantidad = l.Cantidad,
                        PrecioUnitarioUsd = query.Publico ? null : l.PrecioUnitarioUsd,
                        SubtotalUsd = query.Publico ? null : l.SubtotalUsd,
                    }).ToList(),
                };
            }

            // ── Bitácora ──
            var obs = await _consultas.ConsultarAsync<FilaObservacion>(SqlObservaciones, new { EnvioId = cab.EnvioId });
            dto.Observaciones = obs.Select(o => new SeguimientoObservacionDto
            {
                Id = o.Id,
                FechaHora = o.FechaHora,
                Usuario = query.Publico ? null : o.Usuario,
                Texto = o.Texto,
            }).ToList();

            return dto;
        }

        private static List<SeguimientoEtapaDto> ArmarEtapas(FilaCabecera cab, string estado, Dictionary<string, FilaSla> sla, DateTime hoy)
        {
            DateTime?[] fechas = { cab.FechaRecibido, cab.FechaFactura, cab.FechaEnvio, cab.FechaEntrega };
            // Índice del último hito alcanzado. 'anulado' (o un estado desconocido) = solo 'recibido'.
            var indice = Array.IndexOf(Estados, estado);
            if (indice < 0) indice = 0;
            var enCurso = estado != "entregado" && estado != "anulado" && Array.IndexOf(Estados, estado) >= 0;
            // El hito "actual": el que está en curso si el envío sigue vivo; si no, el último alcanzado.
            var actual = enCurso ? indice + 1 : indice;

            var etapas = new List<SeguimientoEtapaDto>();
            for (var i = 0; i < Claves.Length; i++)
            {
                var cumplida = i <= indice;
                var e = new SeguimientoEtapaDto
                {
                    Clave = Claves[i],
                    Label = Labels[i],
                    Cumplida = cumplida,
                    Actual = i == actual,
                    Fecha = cumplida ? fechas[i] : null,
                };

                var etapaSla = EtapasSla[i];
                if (etapaSla != null && sla.TryGetValue(etapaSla, out var p))
                {
                    e.UmbralDias = p.Umbral;
                    e.LimiteDias = p.Limite;
                }

                if (i == 0) { etapas.Add(e); continue; }

                if (cumplida)
                {
                    // Cuánto tardó: del hito anterior a este.
                    var desde = fechas[i - 1]?.Date;
                    var hasta = fechas[i]?.Date;
                    if (desde != null && hasta != null)
                        e.DiasTranscurridos = Math.Max(0, (hasta.Value - desde.Value).Days);
                }
                else if (i == actual && enCurso)
                {
                    // Etapa pendiente: desde el último hito alcanzado hasta hoy.
                    var desde = fechas[indice]?.Date;
                    if (desde != null)
                        e.DiasTranscurridos = Math.Max(0, (hoy - desde.Value).Days);
                }

                if (e.DiasTranscurridos != null && e.UmbralDias != null && e.LimiteDias != null)
                    e.EstadoSla = Semaforo(e.DiasTranscurridos.Value, e.UmbralDias.Value, e.LimiteDias.Value);

                etapas.Add(e);
            }
            return etapas;
        }

        private static string Semaforo(int dias, int umbral, int limite)
        {
            if (dias <= umbral) return "ok";
            if (dias <= limite) return "advertencia";
            return "vencido";
        }

        private static string LabelEstado(string estado) => estado switch
        {
            "recibido" => "Recibido",
            "facturado" => "Confirmado",   // descriptor de Envio: 'facturado' se lee "Confirmado"
            "despachado" => "Despachado",
            "entregado" => "Entregado",
            "anulado" => "Anulado",
            _ => estado,
        };

        // ── Filas crudas (Dapper mapea por nombre de columna) ──
        private class FilaCabecera
        {
            public int EnvioId { get; set; }
            public string? CodigoRastreo { get; set; }
            public string? Estado { get; set; }
            public string? MotivoAnulacion { get; set; }
            public DateTime? FechaRecibido { get; set; }
            public DateTime? FechaFactura { get; set; }
            public DateTime? FechaEnvio { get; set; }
            public DateTime? FechaEntrega { get; set; }
            public int? PedidoId { get; set; }
            public int ClienteId { get; set; }
            public string? ClienteNombre { get; set; }
            public string? ClienteCiudad { get; set; }
            public string? ClienteDireccion { get; set; }
            public string? ClienteTelefono { get; set; }
            public int AgenciaId { get; set; }
            public string? AgenciaNombre { get; set; }
        }

        private class FilaSla
        {
            public string? Etapa { get; set; }
            public int Umbral { get; set; }
            public int Limite { get; set; }
        }

        private class FilaPedido
        {
            public int Id { get; set; }
            public string? Numero { get; set; }
            public DateTime Fecha { get; set; }
            public string? Estado { get; set; }
            public string? VendedorNombre { get; set; }
            public decimal TotalUsd { get; set; }
        }

        private class FilaLinea
        {
            public int VarianteId { get; set; }
            public string? Sku { get; set; }
            public string? ProductoNombre { get; set; }
            public int? ImagenPrincipalId { get; set; }
            public string? Talla { get; set; }
            public string? Color { get; set; }
            public decimal Cantidad { get; set; }
            public decimal PrecioUnitarioUsd { get; set; }
            public decimal SubtotalUsd { get; set; }
        }

        private class FilaObservacion
        {
            public int Id { get; set; }
            public DateTime FechaHora { get; set; }
            public string? Usuario { get; set; }
            public string? Texto { get; set; }
        }
    }
}
