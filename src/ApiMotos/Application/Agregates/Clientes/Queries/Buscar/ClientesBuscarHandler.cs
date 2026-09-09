using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Clientes.Queries.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Queries.Buscar
{
    public class ClientesBuscarHandler : GenericBuscarHandler<ClientesBuscarQuery, ClientesDto>
    {
        private const string Sql = @"SELECT TOP 10 e.*, vendedor.Nombre AS VendedorDisplay
                FROM PC_CLIENTES e
                LEFT JOIN PC_VENDEDORES vendedor ON e.VendedorId = vendedor.Id
                WHERE e.Nombre LIKE @q OR e.Telefono LIKE @q OR e.DireccionEntrega LIKE @q
                   OR e.Ciudad LIKE @q OR e.Contacto LIKE @q
                ORDER BY e.Id DESC";

        public ClientesBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
