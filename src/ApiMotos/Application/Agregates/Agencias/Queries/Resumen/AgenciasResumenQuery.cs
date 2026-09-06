using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Agencias.Queries.Resumen
{
    public record AgenciasResumenQuery() : IQuery<AgenciasResumenDto>;

    public class AgenciasResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoAgencia> PorEstado { get; set; } = new();
        public List<GrupoConteoAgencia> PorMes { get; set; } = new();
    }

    public class GrupoConteoAgencia
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
