using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Envios.Queries.Envios;

namespace ApiMotos.Application.Agregates.Envios.Queries.ByAgenciaId
{
    public class EnviosByAgenciaIdHandler : GenericPorFkHandler<EnviosByAgenciaIdQuery, EnviosDto>
    {
        private const string Sql = @"
SELECT e.*
    , cliente.Nombre AS ClienteDisplay
    , agencia.Nombre AS AgenciaDisplay
FROM PC_ENVIOS e
LEFT JOIN PC_CLIENTES cliente ON e.ClienteId = cliente.Id
LEFT JOIN PC_AGENCIAS agencia ON e.AgenciaId = agencia.Id
WHERE e.AgenciaId = @AgenciaId";

        public EnviosByAgenciaIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { AgenciaId = q.AgenciaId })
        {
        }
    }
}
