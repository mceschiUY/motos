using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Variantes.Queries.Resumen
{
    public record VariantesResumenQuery() : IQuery<VariantesResumenDto>;

    public class VariantesResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoVariante> PorEstado { get; set; } = new();
        public List<GrupoConteoVariante> PorMes { get; set; } = new();
    }

    public class GrupoConteoVariante
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
