using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones
{
    public class ObservacionesHandler : GenericListaHandler<ObservacionesQuery, ObservacionesDto>
    {
        private const string Sql = @"
SELECT e.*
    , envio.CodigoRastreo AS EnvioDisplay
FROM PC_OBSERVACIONES e
LEFT JOIN PC_ENVIOS envio ON e.EnvioId = envio.Id";

        public ObservacionesHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
