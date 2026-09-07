using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Depositos.Queries.Resumen
{
    public class DepositosResumenHandler : GenericResumenHandler<DepositosResumenQuery, DepositosResumenDto, GrupoConteoDeposito>
    {
        public DepositosResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_DEPOSITOS",
                null,
                null,
                (total, porEstado, porMes) => new DepositosResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
