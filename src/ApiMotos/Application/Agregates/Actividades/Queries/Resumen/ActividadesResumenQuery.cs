using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Actividades.Queries.Resumen
{
    public record ActividadesResumenQuery() : IQuery<ActividadesResumenDto>;

    public class ActividadesResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoActividad> PorEstado { get; set; } = new();
        public List<GrupoConteoActividad> PorMes { get; set; } = new();
    }

    public class GrupoConteoActividad
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
