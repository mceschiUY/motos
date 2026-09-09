using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Vendedores.Queries.Vendedores
{
    public record VendedoresQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<VendedoresDto>>;
}
