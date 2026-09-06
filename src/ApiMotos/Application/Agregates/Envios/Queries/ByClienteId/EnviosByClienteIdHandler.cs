using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Envios.Queries.Envios;

namespace ApiMotos.Application.Agregates.Envios.Queries.ByClienteId
{
    public class EnviosByClienteIdHandler : GenericPorFkHandler<EnviosByClienteIdQuery, EnviosDto>
    {
        private const string Sql = @"
SELECT e.*
    , cliente.Nombre AS ClienteDisplay
    , agencia.Nombre AS AgenciaDisplay
FROM PC_ENVIOS e
LEFT JOIN PC_CLIENTES cliente ON e.ClienteId = cliente.Id
LEFT JOIN PC_AGENCIAS agencia ON e.AgenciaId = agencia.Id
WHERE e.ClienteId = @ClienteId";

        public EnviosByClienteIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { ClienteId = q.ClienteId })
        {
        }
    }
}
