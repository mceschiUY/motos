using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Productos.Queries.Resumen
{
    public record ProductosResumenQuery() : IQuery<ProductosResumenDto>;

    public class ProductosResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoProducto> PorEstado { get; set; } = new();
        public List<GrupoConteoProducto> PorMes { get; set; } = new();
    }

    public class GrupoConteoProducto
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
