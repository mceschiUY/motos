using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Actividades.Queries.Actividades
{
    public record ActividadesQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<ActividadesDto>>;
}
