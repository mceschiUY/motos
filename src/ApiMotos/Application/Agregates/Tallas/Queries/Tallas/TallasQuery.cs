using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Tallas.Queries.Tallas
{
    public record TallasQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<TallasDto>>;
}
