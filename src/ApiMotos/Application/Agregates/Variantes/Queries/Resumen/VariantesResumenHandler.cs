using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Variantes.Queries.Resumen
{
    public class VariantesResumenHandler : GenericResumenHandler<VariantesResumenQuery, VariantesResumenDto, GrupoConteoVariante>
    {
        public VariantesResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_VARIANTES",
                null,
                null,
                (total, porEstado, porMes) => new VariantesResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
