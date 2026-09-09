using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Pedidos.Queries.Pedidos
{
    public record PedidosQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<PedidosDto>>;
}
