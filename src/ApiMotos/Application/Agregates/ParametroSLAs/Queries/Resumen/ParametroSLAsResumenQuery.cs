using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Queries.Resumen
{
    public record ParametroSLAsResumenQuery() : IQuery<ParametroSLAsResumenDto>;

    public class ParametroSLAsResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoParametroSLA> PorEstado { get; set; } = new();
        public List<GrupoConteoParametroSLA> PorMes { get; set; } = new();
    }

    public class GrupoConteoParametroSLA
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
