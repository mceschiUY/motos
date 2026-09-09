using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Clientes.Queries.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Queries.ByVendedorId
{
    public class ClientesByVendedorIdHandler : GenericPorFkHandler<ClientesByVendedorIdQuery, ClientesDto>
    {
        private const string Sql = @"
SELECT e.*
    , vendedor.Nombre AS VendedorDisplay
FROM PC_CLIENTES e
LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
WHERE e.VendedorId = @VendedorId
ORDER BY e.Ciudad, e.Nombre";

        public ClientesByVendedorIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { VendedorId = q.VendedorId })
        {
        }
    }
}
