using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Metas.Queries.Metas
{
    public record MetasQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<MetasDto>>;
}
