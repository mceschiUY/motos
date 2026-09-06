using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Envios.Queries.Resumen
{
    public record EnviosResumenQuery() : IQuery<EnviosResumenDto>;

    public class EnviosResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoEnvio> PorEstado { get; set; } = new();
        public List<GrupoConteoEnvio> PorMes { get; set; } = new();
    }

    public class GrupoConteoEnvio
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
