using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Comercial.Agenda
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Une en una sola lista las actividades PLANIFICADAS (ProximaAccion
    /// dentro del rango) y las REALIZADAS (Fecha dentro del rango), con los datos del cliente
    /// (ciudad y coordenadas para la ruta del día) y del vendedor. SQL crudo vía IQueryService,
    /// como ExistenciasHandler.
    /// </summary>
    public class AgendaHandler : IRequestHandler<AgendaQuery, List<AgendaDto>>
    {
        private const string Sql = @"
SELECT CAST(a.ProximaAccion AS datetime2) AS Fecha
     , N'planificada' AS Tipo
     , a.Id AS ActividadId
     , a.ClienteId, c.Nombre AS ClienteDisplay, c.Ciudad, c.Latitud, c.Longitud
     , a.VendedorId, v.Nombre AS VendedorDisplay
     , a.Tipo AS TipoActividad, a.Resultado, a.Notas
     , c.Telefono
     , CASE WHEN ua.Ultima IS NULL THEN NULL ELSE DATEDIFF(DAY, ua.Ultima, @Hoy) END AS DiasSinVisita
     , up.Fecha AS UltimoPedidoFecha, up.TotalUsd AS UltimoPedidoTotalUsd
     , ISNULL(pa.Abiertos, 0) AS PedidosAbiertos
FROM PC_ACTIVIDADES a
LEFT JOIN PC_CLIENTES c ON c.Id = a.ClienteId
LEFT JOIN PC_VENDEDORES v ON v.Id = a.VendedorId
OUTER APPLY (SELECT MAX(x.Fecha) AS Ultima FROM PC_ACTIVIDADES x WHERE x.ClienteId = a.ClienteId AND x.Fecha <= @Hoy) ua
OUTER APPLY (SELECT TOP 1 p.Fecha, p.TotalUsd FROM PC_PEDIDOS p WHERE p.ClienteId = a.ClienteId AND p.Estado <> N'anulado' ORDER BY p.Fecha DESC, p.Id DESC) up
OUTER APPLY (SELECT COUNT(*) AS Abiertos FROM PC_PEDIDOS p WHERE p.ClienteId = a.ClienteId AND p.Estado IN (N'borrador', N'confirmado', N'preparado', N'despachado')) pa
WHERE a.ProximaAccion IS NOT NULL
  AND a.ProximaAccion >= @Desde AND a.ProximaAccion <= @Hasta
  AND (@VendedorId IS NULL OR a.VendedorId = @VendedorId)
UNION ALL
SELECT CAST(CAST(a.Fecha AS date) AS datetime2) AS Fecha
     , N'realizada' AS Tipo
     , a.Id AS ActividadId
     , a.ClienteId, c.Nombre AS ClienteDisplay, c.Ciudad, c.Latitud, c.Longitud
     , a.VendedorId, v.Nombre AS VendedorDisplay
     , a.Tipo AS TipoActividad, a.Resultado, a.Notas
     , c.Telefono
     , CASE WHEN ua.Ultima IS NULL THEN NULL ELSE DATEDIFF(DAY, ua.Ultima, @Hoy) END AS DiasSinVisita
     , up.Fecha AS UltimoPedidoFecha, up.TotalUsd AS UltimoPedidoTotalUsd
     , ISNULL(pa.Abiertos, 0) AS PedidosAbiertos
FROM PC_ACTIVIDADES a
LEFT JOIN PC_CLIENTES c ON c.Id = a.ClienteId
LEFT JOIN PC_VENDEDORES v ON v.Id = a.VendedorId
OUTER APPLY (SELECT MAX(x.Fecha) AS Ultima FROM PC_ACTIVIDADES x WHERE x.ClienteId = a.ClienteId AND x.Fecha <= @Hoy) ua
OUTER APPLY (SELECT TOP 1 p.Fecha, p.TotalUsd FROM PC_PEDIDOS p WHERE p.ClienteId = a.ClienteId AND p.Estado <> N'anulado' ORDER BY p.Fecha DESC, p.Id DESC) up
OUTER APPLY (SELECT COUNT(*) AS Abiertos FROM PC_PEDIDOS p WHERE p.ClienteId = a.ClienteId AND p.Estado IN (N'borrador', N'confirmado', N'preparado', N'despachado')) pa
WHERE CAST(a.Fecha AS date) >= @Desde AND CAST(a.Fecha AS date) <= @Hasta
  AND (@VendedorId IS NULL OR a.VendedorId = @VendedorId)
ORDER BY Fecha, Tipo, Ciudad, DiasSinVisita DESC, ClienteDisplay, ActividadId";

        private readonly IQueryService _consultas;

        public AgendaHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public Task<List<AgendaDto>> Handle(AgendaQuery query, CancellationToken cancellationToken)
        {
            var hoy = Clock.Current.Today;
            var desde = (query.Desde ?? hoy).Date;
            var hasta = (query.Hasta ?? hoy.AddDays(7)).Date;
            if (hasta < desde) (desde, hasta) = (hasta, desde);

            return _consultas.ConsultarAsync<AgendaDto>(Sql, new
            {
                VendedorId = query.VendedorId,
                Desde = desde,
                Hasta = hasta,
                Hoy = hoy,
            });
        }
    }
}
