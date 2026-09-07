using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Common.Generated;
using ApiMotos.Application.Agregates.Variantes.Queries.Variantes;

namespace ApiMotos.Application.Agregates.Variantes.Queries.ByProductoId
{
    public class VariantesByProductoIdHandler : GenericPorFkHandler<VariantesByProductoIdQuery, VariantesDto>
    {
        private const string Sql = @"
SELECT e.*
    , producto.Nombre AS ProductoDisplay
    , talla.Nombre AS TallaDisplay
    , color.Nombre AS ColorDisplay
FROM PC_VARIANTES e
LEFT JOIN PC_PRODUCTOS producto ON e.ProductoId = producto.Id
LEFT JOIN PC_TALLAS talla ON e.TallaId = talla.Id
LEFT JOIN PC_COLORES color ON e.ColorId = color.Id
WHERE e.ProductoId = @ProductoId";

        public VariantesByProductoIdHandler(IQueryService consultas)
            : base(consultas, Sql, q => new { ProductoId = q.ProductoId })
        {
        }
    }
}
