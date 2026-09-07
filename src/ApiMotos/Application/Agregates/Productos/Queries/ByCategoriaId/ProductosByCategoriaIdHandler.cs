using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Productos.Queries.Productos;

namespace ApiMotos.Application.Agregates.Productos.Queries.ByCategoriaId
{
    public class ProductosByCategoriaIdHandler : GenericPorFkHandler<ProductosByCategoriaIdQuery, ProductosDto>
    {
        private const string Sql = @"
SELECT e.*
    , marca.Nombre AS MarcaDisplay
    , categoria.Nombre AS CategoriaDisplay
FROM PC_PRODUCTOS e
LEFT JOIN PC_MARCAS marca ON e.MarcaId = marca.Id
LEFT JOIN PC_CATEGORIAS categoria ON e.CategoriaId = categoria.Id
WHERE e.CategoriaId = @CategoriaId";

        public ProductosByCategoriaIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { CategoriaId = q.CategoriaId })
        {
        }
    }
}
