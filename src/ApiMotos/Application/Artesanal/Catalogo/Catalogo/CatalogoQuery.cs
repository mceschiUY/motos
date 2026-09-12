using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Catalogo.Catalogo
{
    /// <summary>Catálogo navegable: cards de productos activos, destacados primero.</summary>
    public record CatalogoQuery(int? MarcaId = null, int? CategoriaId = null, string? Q = null) : IQuery<List<CatalogoItemDto>>;
}
