using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Categorias.Queries.Resumen
{
    public record CategoriasResumenQuery() : IQuery<CategoriasResumenDto>;

    public class CategoriasResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoCategoria> PorEstado { get; set; } = new();
        public List<GrupoConteoCategoria> PorMes { get; set; } = new();
    }

    public class GrupoConteoCategoria
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
