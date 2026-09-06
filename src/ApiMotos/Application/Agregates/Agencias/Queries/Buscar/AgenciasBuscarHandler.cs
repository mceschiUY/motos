using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Agencias.Queries.Agencias;

namespace ApiMotos.Application.Agregates.Agencias.Queries.Buscar
{
    public class AgenciasBuscarHandler : GenericBuscarHandler<AgenciasBuscarQuery, AgenciasDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_AGENCIAS
                WHERE Nombre LIKE @q
                ORDER BY Id DESC";

        public AgenciasBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
