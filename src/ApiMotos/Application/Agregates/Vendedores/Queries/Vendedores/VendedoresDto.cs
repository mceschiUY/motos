namespace ApiMotos.Application.Agregates.Vendedores.Queries.Vendedores
{
    public class VendedoresDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Zona { get; set; }
        public decimal ComisionPorcentaje { get; set; }
        public string? Usuario { get; set; }
        public bool Activo { get; set; }
    }
}
