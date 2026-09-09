using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.PedidoLineas.Queries.PedidoLineas
{
    public record PedidoLineasQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<PedidoLineasDto>>;
}
