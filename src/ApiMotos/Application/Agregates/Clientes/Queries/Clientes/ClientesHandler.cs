using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Clientes.Queries.Clientes
{
    public class ClientesHandler : GenericListaHandler<ClientesQuery, ClientesDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_CLIENTES e";

        public ClientesHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
