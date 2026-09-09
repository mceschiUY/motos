using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Vendedores.Queries.Resumen
{
    public class VendedoresResumenHandler : GenericResumenHandler<VendedoresResumenQuery, VendedoresResumenDto, GrupoConteoVendedor>
    {
        public VendedoresResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_VENDEDORES",
                "SELECT ISNULL(Zona, N'(sin zona)') AS Clave, COUNT(*) AS Cantidad FROM PC_VENDEDORES GROUP BY Zona",
                null /* sin campo fecha: PorMes queda vacío */,
                (total, porEstado, porMes) => new VendedoresResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
