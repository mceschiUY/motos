using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Marcas.Queries.Resumen
{
    public record MarcasResumenQuery() : IQuery<MarcasResumenDto>;

    public class MarcasResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoMarca> PorEstado { get; set; } = new();
        public List<GrupoConteoMarca> PorMes { get; set; } = new();
    }

    public class GrupoConteoMarca
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
