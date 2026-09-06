using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Agencias.Queries.Resumen
{
    public class AgenciasResumenHandler : GenericResumenHandler<AgenciasResumenQuery, AgenciasResumenDto, GrupoConteoAgencia>
    {
        public AgenciasResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_AGENCIAS",
                null /* entidad sin campo de estado: PorEstado queda vacío */,
                null /* entidad sin campo fecha: PorMes queda vacío */,
                (total, porEstado, porMes) => new AgenciasResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
