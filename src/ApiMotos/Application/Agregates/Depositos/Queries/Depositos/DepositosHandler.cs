using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Depositos.Queries.Depositos
{
    public class DepositosHandler : GenericListaHandler<DepositosQuery, DepositosDto>
    {
        private const string Sql = @"
SELECT e.*
FROM PC_DEPOSITOS e";

        public DepositosHandler(IQueryService consultas)
            : base(consultas, Sql, q => (q.Id, q.Skip, q.Take))
        {
        }
    }
}
