using MediatR;
using ApiMotos.Application.Agregates.Productos.Queries.Productos;

namespace ApiMotos.Application.Agregates.Productos.Queries.ByMarcaId
{
    public class ProductosByMarcaIdQuery : IRequest<List<ProductosDto>>
    {
        public int MarcaId { get; set; }
    }
}
