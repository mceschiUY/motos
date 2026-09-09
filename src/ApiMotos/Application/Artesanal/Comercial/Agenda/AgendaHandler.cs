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
FROM PC_ACTIVIDADES a
LEFT JOIN PC_CLIENTES c ON c.Id = a.ClienteId
LEFT JOIN PC_VENDEDORES v ON v.Id = a.VendedorId
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
FROM PC_ACTIVIDADES a
LEFT JOIN PC_CLIENTES c ON c.Id = a.ClienteId
LEFT JOIN PC_VENDEDORES v ON v.Id = a.VendedorId
WHERE CAST(a.Fecha AS date) >= @Desde AND CAST(a.Fecha AS date) <= @Hasta
  AND (@VendedorId IS NULL OR a.VendedorId = @VendedorId)
ORDER BY Fecha, Tipo, Ciudad, ClienteDisplay, ActividadId";

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
            });
        }
    }
}
