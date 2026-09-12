using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Comercial.PanelVendedor
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Complementa AvanceVendedor/Agenda/Comisiones con lo que la
    /// escena del vendedor necesita y ninguno de esos devuelve. SQL crudo vía IQueryService.
    /// Criterios (consistentes con AvanceVendedorHandler y ComisionesHandler):
    ///  - "vendido" = pedidos ENTREGADOS por Fecha del pedido en el mes (el ranking usa eso);
    ///  - "ventas por semana" = pedidos distintos de borrador/anulado por Fecha (lo colocado);
    ///  - "comisión proyectada" = total del mes en estado distinto de anulado y de entregado,
    ///    por el % de hoy (la sellada la trae /avance; ésta es la que falta cobrar si entrega
    ///    todo lo que tiene abierto).
    /// </summary>
    public class PanelVendedorHandler : IRequestHandler<PanelVendedorQuery, PanelVendedorDto?>
    {
        private const string SqlVendedor = @"
SELECT v.Id, v.Nombre, v.Zona, v.Telefono, v.Email, v.ComisionPorcentaje,
       CAST(ISNULL(v.Activo, 1) AS BIT) AS Activo, v.Usuario
FROM PC_VENDEDORES v
WHERE v.Id = @VendedorId";

        private const string SqlClientes = @"
SELECT c.Id, c.Nombre, c.Tipo, c.Ciudad
     , DATEDIFF(DAY, (SELECT MAX(a.Fecha) FROM PC_ACTIVIDADES a WHERE a.ClienteId = c.Id), @Hoy) AS DiasSinVisita
     , (SELECT MAX(p.Fecha) FROM PC_PEDIDOS p
        WHERE p.ClienteId = c.Id AND p.Estado NOT IN (N'borrador', N'anulado')) AS UltimoPedidoFecha
     , (SELECT COUNT(*) FROM PC_PEDIDOS p
        WHERE p.ClienteId = c.Id AND p.Estado NOT IN (N'borrador', N'anulado')
          AND p.Fecha >= @InicioAnio AND p.Fecha < @FinAnio) AS PedidosAnio
     , ISNULL((SELECT SUM(p.TotalUsd) FROM PC_PEDIDOS p
        WHERE p.ClienteId = c.Id AND p.Estado NOT IN (N'borrador', N'anulado')
          AND p.Fecha >= @InicioAnio AND p.Fecha < @FinAnio), 0) AS TotalAnioUsd
FROM PC_CLIENTES c
WHERE c.VendedorId = @VendedorId
ORDER BY c.Nombre";

        private const string SqlPedidosAbiertos = @"
SELECT p.Id, p.Numero, p.Fecha, p.Estado, c.Nombre AS ClienteNombre, p.TotalUsd
FROM PC_PEDIDOS p
LEFT JOIN PC_CLIENTES c ON c.Id = p.ClienteId
WHERE p.VendedorId = @VendedorId
  AND p.Estado IN (N'borrador', N'confirmado', N'preparado', N'despachado')
ORDER BY p.Fecha DESC, p.Id DESC";

        private const string SqlPendienteMes = @"
SELECT ISNULL(SUM(p.TotalUsd), 0) AS Total
FROM PC_PEDIDOS p
WHERE p.VendedorId = @VendedorId
  AND p.Estado NOT IN (N'anulado', N'entregado')
  AND p.Fecha >= @Desde AND p.Fecha < @Hasta";

        private const string SqlPedidosSemanas = @"
SELECT CAST(p.Fecha AS date) AS Fecha, p.TotalUsd
FROM PC_PEDIDOS p
WHERE p.VendedorId = @VendedorId
  AND p.Estado NOT IN (N'borrador', N'anulado')
  AND p.Fecha >= @Desde AND p.Fecha < @Hasta";

        // El propio vendedor entra siempre (aunque esté inactivo) para poder ubicarlo.
        private const string SqlRanking = @"
SELECT v.Id AS VendedorId
     , ISNULL((SELECT SUM(p.TotalUsd) FROM PC_PEDIDOS p
               WHERE p.VendedorId = v.Id AND p.Estado = N'entregado'
                 AND p.Fecha >= @Desde AND p.Fecha < @Hasta), 0) AS VendidoUsd
FROM PC_VENDEDORES v
WHERE v.Activo = 1 OR v.Id = @VendedorId
ORDER BY VendidoUsd DESC, v.Nombre, v.Id";

        private sealed class FilaTotal { public decimal Total { get; set; } }
        private sealed class FilaPedidoSemana { public DateTime Fecha { get; set; } public decimal TotalUsd { get; set; } }
        private sealed class FilaRanking { public int VendedorId { get; set; } public decimal VendidoUsd { get; set; } }

        private readonly IQueryService _consultas;

        public PanelVendedorHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<PanelVendedorDto?> Handle(PanelVendedorQuery query, CancellationToken cancellationToken)
        {
            if (query.VendedorId <= 0) throw new ArgumentException("Vendedor inválido");

            var hoy = Clock.Current.Today;
            var desdeMes = new DateTime(hoy.Year, hoy.Month, 1);
            var hastaMes = desdeMes.AddMonths(1);
            var inicioAnio = new DateTime(hoy.Year, 1, 1);
            var finAnio = inicioAnio.AddYears(1);

            var cabecera = (await _consultas.ConsultarAsync<PanelVendedorCabeceraDto>(SqlVendedor, new { VendedorId = query.VendedorId }))
                .FirstOrDefault();
            if (cabecera == null) return null;   // null => 404 en el controller

            var dto = new PanelVendedorDto { Vendedor = cabecera };

            // Cartera con semáforo de contacto.
            dto.Clientes = await _consultas.ConsultarAsync<PanelVendedorClienteDto>(SqlClientes, new
            {
                VendedorId = query.VendedorId,
                Hoy = hoy,
                InicioAnio = inicioAnio,
                FinAnio = finAnio,
            });
            foreach (var c in dto.Clientes)
            {
                c.Semaforo = c.DiasSinVisita == null ? "rojo"
                           : c.DiasSinVisita <= 15 ? "verde"
                           : c.DiasSinVisita <= 30 ? "amarillo"
                           : "rojo";
            }

            dto.PedidosAbiertos = await _consultas.ConsultarAsync<PanelVendedorPedidoDto>(SqlPedidosAbiertos, new { VendedorId = query.VendedorId });

            // Comisión proyectada: lo que cobraría si entrega todo lo que tiene abierto del mes.
            var pendiente = (await _consultas.ConsultarAsync<FilaTotal>(SqlPendienteMes, new
            {
                VendedorId = query.VendedorId,
                Desde = desdeMes,
                Hasta = hastaMes,
            })).FirstOrDefault()?.Total ?? 0m;
            dto.PendienteEntregaUsd = pendiente;
            dto.ComisionProyectadaUsd = Math.Round(pendiente * cabecera.ComisionPorcentaje / 100m, 2, MidpointRounding.AwayFromZero);

            // Ventas por semana: 8 semanas que arrancan en lunes, la actual incluida, siempre 8 filas.
            var lunesActual = hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7));
            var desdeSemanas = lunesActual.AddDays(-7 * 7);
            var pedidosSemanas = await _consultas.ConsultarAsync<FilaPedidoSemana>(SqlPedidosSemanas, new
            {
                VendedorId = query.VendedorId,
                Desde = desdeSemanas,
                Hasta = lunesActual.AddDays(7),
            });
            for (var i = 0; i < 8; i++)
            {
                var inicio = desdeSemanas.AddDays(7 * i);
                var fin = inicio.AddDays(7);
                var deLaSemana = pedidosSemanas.Where(p => p.Fecha >= inicio && p.Fecha < fin).ToList();
                dto.VentasPorSemana.Add(new PanelVendedorSemanaDto
                {
                    SemanaInicio = inicio,
                    TotalUsd = deLaSemana.Sum(p => p.TotalUsd),
                    Pedidos = deLaSemana.Count,
                });
            }

            // Ranking del mes por vendido (entregado), entre vendedores activos.
            var ranking = await _consultas.ConsultarAsync<FilaRanking>(SqlRanking, new
            {
                VendedorId = query.VendedorId,
                Desde = desdeMes,
                Hasta = hastaMes,
            });
            var posicion = ranking.FindIndex(r => r.VendedorId == query.VendedorId);
            dto.Ranking = new PanelVendedorRankingDto
            {
                Posicion = posicion < 0 ? 0 : posicion + 1,
                Total = ranking.Count,
            };

            return dto;
        }
    }
}
