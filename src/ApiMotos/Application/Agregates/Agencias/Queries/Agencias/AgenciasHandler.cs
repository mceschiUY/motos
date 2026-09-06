using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Agencias.Queries.Agencias
{
    public class AgenciasHandler : GenericListaHandler<AgenciasQuery, AgenciasDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_AGENCIAS e";

        public AgenciasHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
