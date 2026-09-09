using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Artesanal.Comercial.ClientesSinVisitar
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Clientes "activos" (hoy todos: no hay flag) con vendedor
    /// asignado cuya última actividad (de cualquier vendedor) es anterior a hoy - N días, o que
    /// nunca tuvieron una. Ordenados del más abandonado al menos.
    /// </summary>
    public class ClientesSinVisitarHandler : IRequestHandler<ClientesSinVisitarQuery, List<ClienteSinVisitarDto>>
    {
        private const string Sql = @"
SELECT c.Id AS ClienteId
     , c.Nombre AS ClienteDisplay
     , c.Ciudad
     , c.VendedorId
     , v.Nombre AS VendedorDisplay
     , ua.Ultima AS UltimaActividad
     , CASE WHEN ua.Ultima IS NULL THEN NULL ELSE DATEDIFF(DAY, ua.Ultima, @Hoy) END AS DiasSinVisita
FROM PC_CLIENTES c
JOIN PC_VENDEDORES v ON v.Id = c.VendedorId
OUTER APPLY (SELECT MAX(a.Fecha) AS Ultima FROM PC_ACTIVIDADES a WHERE a.ClienteId = c.Id) ua
WHERE c.VendedorId IS NOT NULL
  AND (ua.Ultima IS NULL OR ua.Ultima < DATEADD(DAY, -@Dias, @Hoy))
ORDER BY CASE WHEN ua.Ultima IS NULL THEN 0 ELSE 1 END, ua.Ultima, c.Nombre";

        private readonly IQueryService _consultas;

        public ClientesSinVisitarHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public Task<List<ClienteSinVisitarDto>> Handle(ClientesSinVisitarQuery query, CancellationToken cancellationToken)
        {
            var dias = query.Dias <= 0 ? 30 : query.Dias;
            return _consultas.ConsultarAsync<ClienteSinVisitarDto>(Sql, new { Dias = dias, Hoy = Clock.Current.Today });
        }
    }
}
