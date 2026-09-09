using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Pedidos.Queries.Pedidos;

namespace ApiMotos.Application.Agregates.Pedidos.Queries.Buscar
{
    public record PedidosBuscarQuery(string Texto) : IQuery<List<PedidosDto>>;
}
