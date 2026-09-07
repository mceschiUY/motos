using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Depositos.Queries.Depositos;

namespace ApiMotos.Application.Agregates.Depositos.Queries.Buscar
{
    public class DepositosBuscarHandler : GenericBuscarHandler<DepositosBuscarQuery, DepositosDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_DEPOSITOS
                WHERE Codigo LIKE @q OR Nombre LIKE @q
                ORDER BY Id DESC";

        public DepositosBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
