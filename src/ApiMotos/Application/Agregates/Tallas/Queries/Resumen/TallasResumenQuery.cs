using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Tallas.Queries.Resumen
{
    public record TallasResumenQuery() : IQuery<TallasResumenDto>;

    public class TallasResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoTalla> PorEstado { get; set; } = new();
        public List<GrupoConteoTalla> PorMes { get; set; } = new();
    }

    public class GrupoConteoTalla
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
