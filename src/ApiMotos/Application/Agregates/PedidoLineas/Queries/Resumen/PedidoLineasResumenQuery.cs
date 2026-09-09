using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.PedidoLineas.Queries.Resumen
{
    public record PedidoLineasResumenQuery() : IQuery<PedidoLineasResumenDto>;

    public class PedidoLineasResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoPedidoLinea> PorEstado { get; set; } = new();
        public List<GrupoConteoPedidoLinea> PorMes { get; set; } = new();
    }

    public class GrupoConteoPedidoLinea
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
