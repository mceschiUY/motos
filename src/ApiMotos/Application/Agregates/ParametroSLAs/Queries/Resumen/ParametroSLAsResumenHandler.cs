using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Queries.Resumen
{
    public class ParametroSLAsResumenHandler : GenericResumenHandler<ParametroSLAsResumenQuery, ParametroSLAsResumenDto, GrupoConteoParametroSLA>
    {
        public ParametroSLAsResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_PARAMETROSLAS",
                @"SELECT ISNULL(CAST(Etapa AS nvarchar(100)), '') AS Clave, COUNT(*) AS Cantidad FROM PC_PARAMETROSLAS GROUP BY Etapa",
                null /* entidad sin campo fecha: PorMes queda vacío */,
                (total, porEstado, porMes) => new ParametroSLAsResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
