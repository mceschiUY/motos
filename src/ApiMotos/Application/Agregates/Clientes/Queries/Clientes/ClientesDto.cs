namespace ApiMotos.Application.Agregates.Clientes.Queries.Clientes
{
    public class ClientesDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string DireccionEntrega { get; set; }

        // Etapa A (plan §3.2 / §3.8): datos comerciales, todos opcionales.
        public string? Tipo { get; set; }
        public string? Ciudad { get; set; }
        public string? Contacto { get; set; }
        public string? Email { get; set; }
        public int? VendedorId { get; set; }
        public string? VendedorDisplay { get; set; }
        public string? Notas { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }
}
