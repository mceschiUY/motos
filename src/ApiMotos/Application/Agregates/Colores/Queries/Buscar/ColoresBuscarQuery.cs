using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Colores.Queries.Colores;

namespace ApiMotos.Application.Agregates.Colores.Queries.Buscar
{
    public record ColoresBuscarQuery(string Texto) : IQuery<List<ColoresDto>>;
}
