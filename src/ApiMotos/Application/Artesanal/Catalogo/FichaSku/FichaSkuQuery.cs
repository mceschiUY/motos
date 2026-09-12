using ApiMotos.Application.Common;

namespace ApiMotos.Application.Artesanal.Catalogo.FichaSku
{
    /// <summary>Ficha de un SKU (variante): cabecera, stock por depósito, Kardex con saldo corrido y pedidos abiertos.</summary>
    public record FichaSkuQuery(int VarianteId) : IQuery<FichaSkuDto?>;
}
