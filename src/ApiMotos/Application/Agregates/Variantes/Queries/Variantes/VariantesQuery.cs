using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Variantes.Queries.Variantes
{
    public record VariantesQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<VariantesDto>>;
}
