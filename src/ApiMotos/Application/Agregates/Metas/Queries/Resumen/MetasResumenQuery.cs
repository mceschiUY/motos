using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Metas.Queries.Resumen
{
    public record MetasResumenQuery() : IQuery<MetasResumenDto>;

    public class MetasResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoMeta> PorEstado { get; set; } = new();
        public List<GrupoConteoMeta> PorMes { get; set; } = new();
    }

    public class GrupoConteoMeta
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
