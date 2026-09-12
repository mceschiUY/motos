using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Catalogo.FichaProducto
{
    /// <summary>Ficha comercial del producto. DepositoId null = existencias de todos los depositos.</summary>
    public record FichaProductoQuery(int ProductoId, int? DepositoId = null) : IQuery<FichaProductoDto?>;
}
