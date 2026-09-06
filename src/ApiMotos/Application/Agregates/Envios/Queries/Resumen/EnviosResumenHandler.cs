using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Envios.Queries.Resumen
{
    public class EnviosResumenHandler : GenericResumenHandler<EnviosResumenQuery, EnviosResumenDto, GrupoConteoEnvio>
    {
        public EnviosResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_ENVIOS",
                @"SELECT ISNULL(CAST(Estado AS nvarchar(100)), '') AS Clave, COUNT(*) AS Cantidad FROM PC_ENVIOS GROUP BY Estado",
                @"SELECT FORMAT(FechaRecibido, 'yyyy-MM') AS Clave, COUNT(*) AS Cantidad FROM PC_ENVIOS
                   WHERE FechaRecibido IS NOT NULL AND FechaRecibido >= DATEADD(month, -11, GETDATE())
                   GROUP BY FORMAT(FechaRecibido, 'yyyy-MM') ORDER BY Clave",
                (total, porEstado, porMes) => new EnviosResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
