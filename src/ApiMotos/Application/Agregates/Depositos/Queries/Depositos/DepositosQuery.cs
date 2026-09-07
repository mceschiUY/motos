using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Depositos.Queries.Depositos
{
    public record DepositosQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<DepositosDto>>;
}
