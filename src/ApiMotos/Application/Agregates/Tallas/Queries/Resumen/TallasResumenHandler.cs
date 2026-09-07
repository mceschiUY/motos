using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Tallas.Queries.Resumen
{
    public class TallasResumenHandler : GenericResumenHandler<TallasResumenQuery, TallasResumenDto, GrupoConteoTalla>
    {
        public TallasResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_TALLAS",
                null,
                null,
                (total, porEstado, porMes) => new TallasResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
