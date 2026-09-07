using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Marcas.Queries.Marcas
{
    public record MarcasQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<MarcasDto>>;
}
