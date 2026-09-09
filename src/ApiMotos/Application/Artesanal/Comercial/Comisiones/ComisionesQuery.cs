using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Comercial.Comisiones
{
    /// <summary>Liquidación de comisiones de un período YYYY-MM (default: mes actual).</summary>
    public record ComisionesQuery(string? Periodo = null, int? VendedorId = null) : IQuery<List<ComisionVendedorDto>>;
}
