using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Pedidos.Queries.Resumen
{
    public record PedidosResumenQuery() : IQuery<PedidosResumenDto>;

    public class PedidosResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoPedido> PorEstado { get; set; } = new();
        public List<GrupoConteoPedido> PorMes { get; set; } = new();
    }

    public class GrupoConteoPedido
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
