using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Actividades.Queries.Actividades;

namespace ApiMotos.Application.Agregates.Actividades.Queries.Buscar
{
    public record ActividadesBuscarQuery(string Texto) : IQuery<List<ActividadesDto>>;
}
