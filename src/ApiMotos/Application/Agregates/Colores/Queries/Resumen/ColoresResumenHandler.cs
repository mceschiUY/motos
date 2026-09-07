using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Colores.Queries.Resumen
{
    public class ColoresResumenHandler : GenericResumenHandler<ColoresResumenQuery, ColoresResumenDto, GrupoConteoColor>
    {
        public ColoresResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_COLORES",
                null,
                null,
                (total, porEstado, porMes) => new ColoresResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
