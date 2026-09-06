using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Clientes.Queries.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Queries.Buscar
{
    public record ClientesBuscarQuery(string Texto) : IQuery<List<ClientesDto>>;
}
