using System.Globalization;
using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Domain.Agregates.Metas;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Comercial.AvanceVendedor
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Una sola fila con los KPIs del vendedor en el período:
    /// meta, actividades/visitas/cierres del mes, clientes asignados y clientes sin visitar
    /// en 30 días. Devuelve null si el vendedor no existe.
    /// </summary>
    public class AvanceVendedorHandler : IRequestHandler<AvanceVendedorQuery, AvanceVendedorDto?>
    {
        private sealed class Fila
        {
            public int VendedorId { get; set; }
            public decimal ObjetivoUsd { get; set; }
            public decimal VendidoUsd { get; set; }
            public decimal ComisionUsd { get; set; }
            public int Actividades { get; set; }
            public int Visitas { get; set; }
            public int ConPedido { get; set; }
            public int ClientesAsignados { get; set; }
            public int ClientesSinVisitar30d { get; set; }
        }

        private const string Sql = @"
SELECT v.Id AS VendedorId
     , ISNULL((SELECT TOP 1 m.ObjetivoUsd FROM PC_METAS m WHERE m.VendedorId = v.Id AND m.Periodo = @Periodo), 0) AS ObjetivoUsd
     , ISNULL((SELECT SUM(p.TotalUsd) FROM PC_PEDIDOS p
               WHERE p.VendedorId = v.Id AND p.Estado = N'entregado'
                 AND p.Fecha >= @Desde AND p.Fecha < @Hasta), 0) AS VendidoUsd
     , ISNULL((SELECT SUM(p.ComisionUsd) FROM PC_PEDIDOS p
               WHERE p.VendedorId = v.Id AND p.Estado = N'entregado'
                 AND p.Fecha >= @Desde AND p.Fecha < @Hasta), 0) AS ComisionUsd
     , (SELECT COUNT(*) FROM PC_ACTIVIDADES a WHERE a.VendedorId = v.Id AND a.Fecha >= @Desde AND a.Fecha < @Hasta) AS Actividades
     , (SELECT COUNT(*) FROM PC_ACTIVIDADES a WHERE a.VendedorId = v.Id AND a.Fecha >= @Desde AND a.Fecha < @Hasta AND a.Tipo = N'visita') AS Visitas
     , (SELECT COUNT(*) FROM PC_ACTIVIDADES a WHERE a.VendedorId = v.Id AND a.Fecha >= @Desde AND a.Fecha < @Hasta AND a.Resultado = N'pedido') AS ConPedido
     , (SELECT COUNT(*) FROM PC_CLIENTES c WHERE c.VendedorId = v.Id) AS ClientesAsignados
     , (SELECT COUNT(*) FROM PC_CLIENTES c
        WHERE c.VendedorId = v.Id
          AND NOT EXISTS (SELECT 1 FROM PC_ACTIVIDADES a
                          WHERE a.ClienteId = c.Id AND a.Fecha >= DATEADD(DAY, -30, @Hoy))) AS ClientesSinVisitar30d
FROM PC_VENDEDORES v
WHERE v.Id = @VendedorId";

        private readonly IQueryService _consultas;

        public AvanceVendedorHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<AvanceVendedorDto?> Handle(AvanceVendedorQuery query, CancellationToken cancellationToken)
        {
            var hoy = Clock.Current.Today;
            var periodo = string.IsNullOrWhiteSpace(query.Periodo) ? hoy.ToString("yyyy-MM") : query.Periodo.Trim();
            if (!Meta.PeriodoValido.IsMatch(periodo))
                throw new ArgumentException("Periodo inválido: debe tener el formato YYYY-MM (ej. 2026-09)");

            var desde = DateTime.ParseExact(periodo + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var hasta = desde.AddMonths(1);

            var filas = await _consultas.ConsultarAsync<Fila>(Sql, new
            {
                VendedorId = query.VendedorId,
                Periodo = periodo,
                Desde = desde,
                Hasta = hasta,
                Hoy = hoy,
            });
            var f = filas.FirstOrDefault();
            if (f == null) return null;

            return new AvanceVendedorDto
            {
                VendedorId = f.VendedorId,
                Periodo = periodo,
                ObjetivoUsd = f.ObjetivoUsd,
                // Etapa B: pedidos ENTREGADOS del vendedor en el período (lo que se devengó).
                VendidoUsd = f.VendidoUsd,
                ComisionUsd = f.ComisionUsd,
                Actividades = f.Actividades,
                Visitas = f.Visitas,
                ConPedido = f.ConPedido,
                TasaCierre = f.Actividades == 0 ? 0m : Math.Round((decimal)f.ConPedido / f.Actividades, 4),
                ClientesAsignados = f.ClientesAsignados,
                ClientesSinVisitar30d = f.ClientesSinVisitar30d,
            };
        }
    }
}
