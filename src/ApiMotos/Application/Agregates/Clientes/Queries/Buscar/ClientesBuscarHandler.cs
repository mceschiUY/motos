using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Clientes.Queries.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Queries.Buscar
{
    public class ClientesBuscarHandler : GenericBuscarHandler<ClientesBuscarQuery, ClientesDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_CLIENTES
                WHERE Nombre LIKE @q OR Telefono LIKE @q OR DireccionEntrega LIKE @q
                ORDER BY Id DESC";

        public ClientesBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
