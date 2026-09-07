using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Productos.Queries.Productos
{
    public record ProductosQuery(int? Id = null, int? Skip = null, int? Take = null) : IQuery<List<ProductosDto>>;
}
