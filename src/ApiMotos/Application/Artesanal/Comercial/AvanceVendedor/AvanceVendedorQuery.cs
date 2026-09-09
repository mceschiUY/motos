using MediatR;

namespace ApiMotos.Application.Artesanal.Comercial.AvanceVendedor
{
    /// <summary>Avance del vendedor en el período `YYYY-MM` (default: mes actual).</summary>
    public record AvanceVendedorQuery(int VendedorId, string? Periodo = null) : IRequest<AvanceVendedorDto?>;
}
