using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Comercial.PanelVendedor
{
    /// <summary>Panel del vendedor (escena "lo mío"): cartera, pedidos abiertos, proyección y ranking del mes.</summary>
    public record PanelVendedorQuery(int VendedorId) : IQuery<PanelVendedorDto?>;
}
