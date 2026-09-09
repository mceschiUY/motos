using MediatR;
using ApiMotos.Application.Agregates.Clientes.Queries.Clientes;

namespace ApiMotos.Application.Agregates.Clientes.Queries.ByVendedorId
{
    public class ClientesByVendedorIdQuery : IRequest<List<ClientesDto>>
    {
        public int VendedorId { get; set; }
    }
}
