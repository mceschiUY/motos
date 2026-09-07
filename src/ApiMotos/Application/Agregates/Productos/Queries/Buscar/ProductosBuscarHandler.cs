using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Productos.Queries.Productos;

namespace ApiMotos.Application.Agregates.Productos.Queries.Buscar
{
    public class ProductosBuscarHandler : GenericBuscarHandler<ProductosBuscarQuery, ProductosDto>
    {
        private const string Sql = @"SELECT TOP 10 * FROM PC_PRODUCTOS
                WHERE Codigo LIKE @q OR Nombre LIKE @q
                ORDER BY Id DESC";

        public ProductosBuscarHandler(IQueryService consultas)
            : base(consultas, Sql, q => q.Texto)
        {
        }
    }
}
