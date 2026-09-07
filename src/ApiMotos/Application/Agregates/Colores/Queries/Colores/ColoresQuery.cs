using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Colores.Queries.Colores
{
    public record ColoresQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<ColoresDto>>;
}
