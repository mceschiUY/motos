using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones;

namespace ApiMotos.Application.Agregates.Observaciones.Queries.ByEnvioId
{
    public class ObservacionesByEnvioIdHandler : GenericPorFkHandler<ObservacionesByEnvioIdQuery, ObservacionesDto>
    {
        private const string Sql = @"
SELECT e.*
    , envio.CodigoRastreo AS EnvioDisplay
FROM PC_OBSERVACIONES e
LEFT JOIN PC_ENVIOS envio ON e.EnvioId = envio.Id
WHERE e.EnvioId = @EnvioId";

        public ObservacionesByEnvioIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { EnvioId = q.EnvioId })
        {
        }
    }
}
