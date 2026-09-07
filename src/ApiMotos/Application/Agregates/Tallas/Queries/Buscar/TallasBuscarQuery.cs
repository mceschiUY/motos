using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Tallas.Queries.Tallas;

namespace ApiMotos.Application.Agregates.Tallas.Queries.Buscar
{
    public record TallasBuscarQuery(string Texto) : IQuery<List<TallasDto>>;
}
