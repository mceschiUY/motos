using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Vendedores.Queries.Resumen
{
    public record VendedoresResumenQuery() : IQuery<VendedoresResumenDto>;

    public class VendedoresResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoVendedor> PorEstado { get; set; } = new();
        public List<GrupoConteoVendedor> PorMes { get; set; } = new();
    }

    public class GrupoConteoVendedor
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
