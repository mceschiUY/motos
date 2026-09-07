using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Colores.Queries.Resumen
{
    public record ColoresResumenQuery() : IQuery<ColoresResumenDto>;

    public class ColoresResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoColor> PorEstado { get; set; } = new();
        public List<GrupoConteoColor> PorMes { get; set; } = new();
    }

    public class GrupoConteoColor
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
