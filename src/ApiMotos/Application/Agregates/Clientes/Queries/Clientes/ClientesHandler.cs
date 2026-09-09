using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Clientes.Queries.Clientes
{
    public class ClientesHandler : GenericListaHandler<ClientesQuery, ClientesDto>
    {
        private const string Sql = @"
SELECT e.*
    , vendedor.Nombre AS VendedorDisplay
FROM PC_CLIENTES e
LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id";

        public ClientesHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
