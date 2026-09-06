using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Observaciones.Queries.Resumen
{
    public class ObservacionesResumenHandler : GenericResumenHandler<ObservacionesResumenQuery, ObservacionesResumenDto, GrupoConteoObservacion>
    {
        public ObservacionesResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_OBSERVACIONES",
                null /* entidad sin campo de estado: PorEstado queda vacío */,
                @"SELECT FORMAT(FechaHora, 'yyyy-MM') AS Clave, COUNT(*) AS Cantidad FROM PC_OBSERVACIONES
                   WHERE FechaHora IS NOT NULL AND FechaHora >= DATEADD(month, -11, GETDATE())
                   GROUP BY FORMAT(FechaHora, 'yyyy-MM') ORDER BY Clave",
                (total, porEstado, porMes) => new ObservacionesResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
