using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Categorias.Queries.Categorias;

namespace ApiMotos.Application.Agregates.Categorias.Queries.Buscar
{
    public record CategoriasBuscarQuery(string Texto) : IQuery<List<CategoriasDto>>;
}
