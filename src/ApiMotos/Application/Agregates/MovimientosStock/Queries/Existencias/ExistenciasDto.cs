namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.Existencias
{
    /// <summary>Saldo de stock por SKU/depósito, calculado sumando el Kardex (ver doc/modelo-stock.md §5).</summary>
    public class ExistenciasDto
    {
        public int VarianteId { get; set; }
        public string? Sku { get; set; }
        public string? ProductoDisplay { get; set; }
        public int DepositoId { get; set; }
        public string? DepositoDisplay { get; set; }
        public decimal Disponible { get; set; }
    }
}
