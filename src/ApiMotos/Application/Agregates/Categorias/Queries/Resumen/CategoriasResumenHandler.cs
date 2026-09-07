using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Categorias.Queries.Resumen
{
    public class CategoriasResumenHandler : GenericResumenHandler<CategoriasResumenQuery, CategoriasResumenDto, GrupoConteoCategoria>
    {
        public CategoriasResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_CATEGORIAS",
                null,
                null,
                (total, porEstado, porMes) => new CategoriasResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
