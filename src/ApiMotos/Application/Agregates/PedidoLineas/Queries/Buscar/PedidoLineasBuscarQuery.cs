using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.PedidoLineas.Queries.PedidoLineas;

namespace ApiMotos.Application.Agregates.PedidoLineas.Queries.Buscar
{
    public record PedidoLineasBuscarQuery(string Texto) : IQuery<List<PedidoLineasDto>>;
}
