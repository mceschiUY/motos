namespace ApiMotos.Application.Agregates.PedidoLineas.Queries.PedidoLineas
{
    public class PedidoLineasDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string? PedidoDisplay { get; set; }
        public int VarianteId { get; set; }
        public string? VarianteDisplay { get; set; }
        public string? ProductoDisplay { get; set; }
        public string? TallaDisplay { get; set; }
        public string? ColorDisplay { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitarioUsd { get; set; }
        public decimal SubtotalUsd { get; set; }
    }
}
