using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Metas.Queries.Resumen
{
    public class MetasResumenHandler : GenericResumenHandler<MetasResumenQuery, MetasResumenDto, GrupoConteoMeta>
    {
        public MetasResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_METAS",
                null /* sin campo de estado: PorEstado queda vacío */,
                // PorMes = metas por período (el período YA es yyyy-MM).
                "SELECT Periodo AS Clave, COUNT(*) AS Cantidad FROM PC_METAS GROUP BY Periodo ORDER BY Clave",
                (total, porEstado, porMes) => new MetasResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
