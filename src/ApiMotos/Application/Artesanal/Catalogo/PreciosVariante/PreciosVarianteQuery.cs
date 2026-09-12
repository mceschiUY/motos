using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Catalogo.PreciosVariante
{
    /// <summary>Historial de precio y costo de una Variante, del cambio más nuevo al más viejo.</summary>
    public record PreciosVarianteQuery(int VarianteId) : IQuery<List<PrecioVarianteDto>>;
}
