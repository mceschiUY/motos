using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Categorias.Queries.Categorias
{
    public record CategoriasQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<CategoriasDto>>;
}
