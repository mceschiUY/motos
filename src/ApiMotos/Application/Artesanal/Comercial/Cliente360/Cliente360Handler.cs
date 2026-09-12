using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Artesanal.Comun;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Comercial.Cliente360
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Cuatro consultas: cabecera + salud (una fila), las tres
    /// fuentes de la línea de tiempo (actividades, pedidos, envíos) que se mezclan en memoria,
    /// y el top de productos. "Días sin visita" usa el MISMO cálculo que ClientesSinVisitar
    /// (DATEDIFF(DAY, última actividad de cualquier tipo, hoy)).
    /// </summary>
    public class Cliente360Handler : IRequestHandler<Cliente360Query, Cliente360Dto?>
    {
        private const string SqlCabecera = @"
SELECT c.Id, c.Nombre, c.Tipo, c.Ciudad, c.DireccionEntrega, c.Contacto, c.Telefono, c.Email,
       c.Notas, c.Latitud, c.Longitud, c.VendedorId,
       v.Nombre AS VendedorNombre, v.Telefono AS VendedorTelefono,
       ua.Fecha AS UltimaActividadFecha, ua.Tipo AS UltimaActividadTipo, ua.Resultado AS UltimaActividadResultado,
       CASE WHEN ua.Fecha IS NULL THEN NULL ELSE DATEDIFF(DAY, ua.Fecha, @Hoy) END AS DiasSinVisita,
       pa.Proxima AS ProximaAccion,
       ISNULL(pan.Cant, 0) AS PedidosAnio, ISNULL(pan.Total, 0) AS TotalAnioUsd,
       ISNULL(p90.Cant, 0) AS Pedidos90,
       ISNULL(ab.Cant, 0) AS PedidosAbiertos,
       ISNULL(ec.Cant, 0) AS EnviosEnCurso,
       up.Id AS UltimoPedidoId, up.Numero AS UltimoPedidoNumero, up.Fecha AS UltimoPedidoFecha,
       up.Estado AS UltimoPedidoEstado, up.TotalUsd AS UltimoPedidoTotalUsd
FROM PC_CLIENTES c
LEFT JOIN PC_VENDEDORES v ON v.Id = c.VendedorId
OUTER APPLY (SELECT TOP 1 a.Fecha, a.Tipo, a.Resultado FROM PC_ACTIVIDADES a
             WHERE a.ClienteId = c.Id ORDER BY a.Fecha DESC, a.Id DESC) ua
OUTER APPLY (SELECT MIN(a.ProximaAccion) AS Proxima FROM PC_ACTIVIDADES a
             WHERE a.ClienteId = c.Id AND a.ProximaAccion >= @Hoy) pa
OUTER APPLY (SELECT COUNT(*) AS Cant, SUM(p.TotalUsd) AS Total FROM PC_PEDIDOS p
             WHERE p.ClienteId = c.Id AND p.Estado <> 'anulado' AND YEAR(p.Fecha) = YEAR(@Hoy)) pan
OUTER APPLY (SELECT COUNT(*) AS Cant FROM PC_PEDIDOS p
             WHERE p.ClienteId = c.Id AND p.Estado <> 'anulado' AND p.Fecha >= DATEADD(DAY, -90, @Hoy)) p90
OUTER APPLY (SELECT COUNT(*) AS Cant FROM PC_PEDIDOS p
             WHERE p.ClienteId = c.Id AND p.Estado IN ('borrador', 'confirmado', 'preparado', 'despachado')) ab
OUTER APPLY (SELECT COUNT(*) AS Cant FROM PC_ENVIOS e
             WHERE e.ClienteId = c.Id AND e.Estado NOT IN ('entregado', 'anulado')) ec
OUTER APPLY (SELECT TOP 1 p.Id, p.Numero, p.Fecha, p.Estado, p.TotalUsd FROM PC_PEDIDOS p
             WHERE p.ClienteId = c.Id ORDER BY p.Fecha DESC, p.Id DESC) up
WHERE c.Id = @ClienteId";

        private const string SqlActividades = @"
SELECT a.Id, a.Fecha, a.Tipo, a.Resultado, a.Notas, a.PedidoId, v.Nombre AS VendedorNombre
FROM PC_ACTIVIDADES a
LEFT JOIN PC_VENDEDORES v ON v.Id = a.VendedorId
WHERE a.ClienteId = @ClienteId";

        private const string SqlPedidos = @"
SELECT p.Id, p.Numero, p.Fecha, p.Estado, p.TotalUsd,
       (SELECT COUNT(*) FROM PC_PEDIDO_LINEAS l WHERE l.PedidoId = p.Id) AS Lineas
FROM PC_PEDIDOS p
WHERE p.ClienteId = @ClienteId";

        private const string SqlEnvios = @"
SELECT e.Id, e.CodigoRastreo, e.Estado, e.FechaRecibido AS Fecha, ag.Nombre AS AgenciaNombre
FROM PC_ENVIOS e
LEFT JOIN PC_AGENCIAS ag ON ag.Id = e.AgenciaId
WHERE e.ClienteId = @ClienteId";

        private const string SqlTopProductos = @"
SELECT TOP 5 pr.Id AS ProductoId, pr.Nombre AS ProductoNombre,
       SUM(l.Cantidad) AS Unidades, SUM(l.SubtotalUsd) AS TotalUsd
FROM PC_PEDIDO_LINEAS l
JOIN PC_PEDIDOS p ON p.Id = l.PedidoId
JOIN PC_VARIANTES va ON va.Id = l.VarianteId
JOIN PC_PRODUCTOS pr ON pr.Id = va.ProductoId
WHERE p.ClienteId = @ClienteId AND p.Estado IN ('despachado', 'entregado')
GROUP BY pr.Id, pr.Nombre
ORDER BY SUM(l.SubtotalUsd) DESC, SUM(l.Cantidad) DESC";

        private readonly IQueryService _consultas;

        public Cliente360Handler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<Cliente360Dto?> Handle(Cliente360Query query, CancellationToken cancellationToken)
        {
            if (query.ClienteId <= 0) throw new ArgumentException("Cliente inválido");

            var parametros = new { ClienteId = query.ClienteId, Hoy = Clock.Current.Today };
            var fila = (await _consultas.ConsultarAsync<FilaCabecera>(SqlCabecera, parametros)).FirstOrDefault();
            if (fila == null) return null;   // null => el controller devuelve 404

            var dto = new Cliente360Dto
            {
                Id = fila.Id, Nombre = fila.Nombre, Tipo = fila.Tipo, Ciudad = fila.Ciudad,
                DireccionEntrega = fila.DireccionEntrega, Contacto = fila.Contacto, Telefono = fila.Telefono,
                Email = fila.Email, Notas = fila.Notas, Latitud = fila.Latitud, Longitud = fila.Longitud,
                VendedorId = fila.VendedorId, VendedorNombre = fila.VendedorNombre, VendedorTelefono = fila.VendedorTelefono,
                DiasSinVisita = fila.DiasSinVisita,
                UltimaActividadFecha = fila.UltimaActividadFecha,
                UltimaActividadTipo = fila.UltimaActividadTipo,
                UltimaActividadResultado = fila.UltimaActividadResultado,
                ProximaAccion = fila.ProximaAccion,
                PedidosAnio = fila.PedidosAnio,
                TotalAnioUsd = fila.TotalAnioUsd,
                TicketPromedioUsd = fila.PedidosAnio == 0
                    ? 0m
                    : Math.Round(fila.TotalAnioUsd / fila.PedidosAnio, 2, MidpointRounding.AwayFromZero),
                PedidosAbiertos = fila.PedidosAbiertos,
                EnviosEnCurso = fila.EnviosEnCurso,
                UltimoPedido = fila.UltimoPedidoId == null ? null : new Cliente360PedidoDto
                {
                    Id = fila.UltimoPedidoId.Value,
                    Numero = fila.UltimoPedidoNumero,
                    Fecha = fila.UltimoPedidoFecha ?? default,
                    Estado = fila.UltimoPedidoEstado,
                    TotalUsd = fila.UltimoPedidoTotalUsd ?? 0m,
                },
            };

            var diasAlerta = await ParametrosAlertas.DiasSinVisitaAsync(_consultas);
            dto.DiasSinVisitaUmbral = diasAlerta;
            (dto.Semaforo, dto.MotivoSemaforo) = CalcularSemaforo(fila.DiasSinVisita, fila.Pedidos90, diasAlerta);

            var actividades = await _consultas.ConsultarAsync<FilaActividad>(SqlActividades, parametros);
            var pedidos = await _consultas.ConsultarAsync<FilaPedido>(SqlPedidos, parametros);
            var envios = await _consultas.ConsultarAsync<FilaEnvio>(SqlEnvios, parametros);

            var timeline = new List<Cliente360TimelineItemDto>();
            timeline.AddRange(actividades.Select(a => new Cliente360TimelineItemDto
            {
                Tipo = "actividad",
                Id = a.Id,
                Fecha = a.Fecha,
                Titulo = string.IsNullOrWhiteSpace(a.VendedorNombre)
                    ? EtiquetaTipo(a.Tipo)
                    : $"{EtiquetaTipo(a.Tipo)} · {a.VendedorNombre}",
                Detalle = DetalleActividad(a),
                Estado = a.Resultado,
                Ruta = $"/actividad/{a.Id}",
            }));
            timeline.AddRange(pedidos.Select(p => new Cliente360TimelineItemDto
            {
                Tipo = "pedido",
                Id = p.Id,
                Fecha = p.Fecha,
                Titulo = $"Pedido {p.Numero}",
                Detalle = $"{p.Lineas} {(p.Lineas == 1 ? "línea" : "líneas")} · US$ {p.TotalUsd:N2}",
                Estado = p.Estado,
                Ruta = $"/pedido/{p.Id}",
            }));
            timeline.AddRange(envios.Select(e => new Cliente360TimelineItemDto
            {
                Tipo = "envio",
                Id = e.Id,
                Fecha = e.Fecha,
                Titulo = $"Envío {e.CodigoRastreo}",
                Detalle = string.IsNullOrWhiteSpace(e.AgenciaNombre) ? null : $"por {e.AgenciaNombre}",
                Estado = e.Estado,
                Ruta = $"/envio/{e.Id}",
            }));
            dto.Timeline = timeline.OrderByDescending(t => t.Fecha).ThenByDescending(t => t.Id).ToList();

            dto.TopProductos = await _consultas.ConsultarAsync<Cliente360TopProductoDto>(SqlTopProductos, parametros);
            return dto;
        }

        /// <summary>Con `dias` = crm.dias_sin_visita (default 30): verde hasta la mitad · amarillo hasta el límite o sin pedidos en 90 días · rojo pasado el límite o nunca visitado.</summary>
        private static (string, string) CalcularSemaforo(int? diasSinVisita, int pedidos90, int dias)
        {
            if (diasSinVisita == null) return ("rojo", "Nunca visitado");
            var d = diasSinVisita.Value;
            if (d > dias) return ("rojo", $"Sin visita hace {d} días");
            if (d > dias / 2) return ("amarillo", $"Sin visita hace {d} días");
            if (pedidos90 == 0) return ("amarillo", "Sin pedidos en los últimos 90 días");
            return ("verde", d == 0 ? "Visitado hoy" : d == 1 ? "Visitado ayer" : $"Visitado hace {d} días");
        }

        private static string EtiquetaTipo(string? tipo) => tipo switch
        {
            "visita" => "Visita",
            "llamada" => "Llamada",
            "whatsapp" => "WhatsApp",
            "email" => "Email",
            _ => string.IsNullOrWhiteSpace(tipo) ? "Actividad" : tipo,
        };

        private static string EtiquetaResultado(string? r) => r switch
        {
            "pedido" => "Con pedido",
            "sin_pedido" => "Sin pedido",
            "reprogramar" => "Reprogramar",
            "sin_contacto" => "Sin contacto",
            _ => r ?? "",
        };

        private static string? DetalleActividad(FilaActividad a)
        {
            var partes = new List<string>();
            var res = EtiquetaResultado(a.Resultado);
            if (!string.IsNullOrWhiteSpace(res)) partes.Add(res);
            if (!string.IsNullOrWhiteSpace(a.Notas)) partes.Add(a.Notas.Trim());
            return partes.Count == 0 ? null : string.Join(" — ", partes);
        }

        // ─── Filas crudas de Dapper (privadas: el contrato público es el Dto) ───
        private class FilaCabecera
        {
            public int Id { get; set; }
            public string? Nombre { get; set; }
            public string? Tipo { get; set; }
            public string? Ciudad { get; set; }
            public string? DireccionEntrega { get; set; }
            public string? Contacto { get; set; }
            public string? Telefono { get; set; }
            public string? Email { get; set; }
            public string? Notas { get; set; }
            public decimal? Latitud { get; set; }
            public decimal? Longitud { get; set; }
            public int? VendedorId { get; set; }
            public string? VendedorNombre { get; set; }
            public string? VendedorTelefono { get; set; }
            public DateTime? UltimaActividadFecha { get; set; }
            public string? UltimaActividadTipo { get; set; }
            public string? UltimaActividadResultado { get; set; }
            public int? DiasSinVisita { get; set; }
            public DateTime? ProximaAccion { get; set; }
            public int PedidosAnio { get; set; }
            public decimal TotalAnioUsd { get; set; }
            public int Pedidos90 { get; set; }
            public int PedidosAbiertos { get; set; }
            public int EnviosEnCurso { get; set; }
            public int? UltimoPedidoId { get; set; }
            public string? UltimoPedidoNumero { get; set; }
            public DateTime? UltimoPedidoFecha { get; set; }
            public string? UltimoPedidoEstado { get; set; }
            public decimal? UltimoPedidoTotalUsd { get; set; }
        }

        private class FilaActividad
        {
            public int Id { get; set; }
            public DateTime Fecha { get; set; }
            public string? Tipo { get; set; }
            public string? Resultado { get; set; }
            public string? Notas { get; set; }
            public int? PedidoId { get; set; }
            public string? VendedorNombre { get; set; }
        }

        private class FilaPedido
        {
            public int Id { get; set; }
            public string? Numero { get; set; }
            public DateTime Fecha { get; set; }
            public string? Estado { get; set; }
            public decimal TotalUsd { get; set; }
            public int Lineas { get; set; }
        }

        private class FilaEnvio
        {
            public int Id { get; set; }
            public string? CodigoRastreo { get; set; }
            public string? Estado { get; set; }
            public DateTime Fecha { get; set; }
            public string? AgenciaNombre { get; set; }
        }
    }
}
