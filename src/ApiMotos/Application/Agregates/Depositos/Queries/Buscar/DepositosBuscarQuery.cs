using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Depositos.Queries.Depositos;

namespace ApiMotos.Application.Agregates.Depositos.Queries.Buscar
{
    public record DepositosBuscarQuery(string Texto) : IQuery<List<DepositosDto>>;
}
