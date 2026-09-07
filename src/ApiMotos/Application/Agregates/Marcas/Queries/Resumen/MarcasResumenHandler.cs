using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Marcas.Queries.Resumen
{
    public class MarcasResumenHandler : GenericResumenHandler<MarcasResumenQuery, MarcasResumenDto, GrupoConteoMarca>
    {
        public MarcasResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_MARCAS",
                null,
                null,
                (total, porEstado, porMes) => new MarcasResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
