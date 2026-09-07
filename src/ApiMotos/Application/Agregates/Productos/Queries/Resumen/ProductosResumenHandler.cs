using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;

namespace ApiMotos.Application.Agregates.Productos.Queries.Resumen
{
    public class ProductosResumenHandler : GenericResumenHandler<ProductosResumenQuery, ProductosResumenDto, GrupoConteoProducto>
    {
        public ProductosResumenHandler(IQueryService consultas)
            : base(consultas,
                "SELECT COUNT(*) FROM PC_PRODUCTOS",
                null,
                null,
                (total, porEstado, porMes) => new ProductosResumenDto { Total = total, PorEstado = porEstado, PorMes = porMes })
        {
        }
    }
}
