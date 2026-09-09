using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Actividades.Queries.Resumen
{
    public class ActividadesResumenHandler : GenericResumenHandler<ActividadesResumenQuery, ActividadesResumenDto, GrupoConteoActividad>
    {
        public ActividadesResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_ACTIVIDADES",
                // "Estado" de una actividad = su resultado (pedido, sin_pedido, reprogramar, sin_contacto).
                "SELECT ISNULL(CAST(Resultado AS nvarchar(100)), '') AS Clave, COUNT(*) AS Cantidad FROM PC_ACTIVIDADES GROUP BY Resultado",
                @"SELECT FORMAT(Fecha, 'yyyy-MM') AS Clave, COUNT(*) AS Cantidad FROM PC_ACTIVIDADES
                   WHERE Fecha >= DATEADD(month, -11, GETDATE())
                   GROUP BY FORMAT(Fecha, 'yyyy-MM') ORDER BY Clave",
                (total, porEstado, porMes) => new ActividadesResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
