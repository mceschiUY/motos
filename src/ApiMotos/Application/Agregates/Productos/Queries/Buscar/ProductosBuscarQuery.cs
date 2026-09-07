using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Productos.Queries.Productos;

namespace ApiMotos.Application.Agregates.Productos.Queries.Buscar
{
    public record ProductosBuscarQuery(string Texto) : IQuery<List<ProductosDto>>;
}
