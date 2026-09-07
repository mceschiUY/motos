using MediatR;
using ApiMotos.Application.Agregates.Productos.Queries.Productos;

namespace ApiMotos.Application.Agregates.Productos.Queries.ByCategoriaId
{
    public class ProductosByCategoriaIdQuery : IRequest<List<ProductosDto>>
    {
        public int CategoriaId { get; set; }
    }
}
