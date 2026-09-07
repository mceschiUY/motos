namespace ApiMotos.Application.Agregates.Variantes.Queries.Variantes
{
    public class VariantesDto
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string? ProductoDisplay { get; set; }
        public int? TallaId { get; set; }
        public string? TallaDisplay { get; set; }
        public int? ColorId { get; set; }
        public string? ColorDisplay { get; set; }
        public string Sku { get; set; }
        public string? CodigoBarras { get; set; }
        public decimal CostoEstandar { get; set; }
        public decimal PrecioLista { get; set; }
        public bool Activo { get; set; }
    }
}
