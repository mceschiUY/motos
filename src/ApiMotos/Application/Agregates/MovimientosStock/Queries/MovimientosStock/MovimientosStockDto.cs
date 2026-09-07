namespace ApiMotos.Application.Agregates.MovimientosStock.Queries.MovimientosStock
{
    public class MovimientosStockDto
    {
        public int Id { get; set; }
        public int VarianteId { get; set; }
        public string? VarianteDisplay { get; set; }
        public int DepositoId { get; set; }
        public string? DepositoDisplay { get; set; }
        public int? DepositoDestinoId { get; set; }
        public string? DepositoDestinoDisplay { get; set; }
        public string Tipo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal? CostoUnitario { get; set; }
        public string? Motivo { get; set; }
        public string? DocumentoOrigen { get; set; }
        public DateTime Fecha { get; set; }
        public string? Usuario { get; set; }
    }
}
