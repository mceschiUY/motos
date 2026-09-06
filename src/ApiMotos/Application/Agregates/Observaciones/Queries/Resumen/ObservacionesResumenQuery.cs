using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Observaciones.Queries.Resumen
{
    public record ObservacionesResumenQuery() : IQuery<ObservacionesResumenDto>;

    public class ObservacionesResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoObservacion> PorEstado { get; set; } = new();
        public List<GrupoConteoObservacion> PorMes { get; set; } = new();
    }

    public class GrupoConteoObservacion
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
