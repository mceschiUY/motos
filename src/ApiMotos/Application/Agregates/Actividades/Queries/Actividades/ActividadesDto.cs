namespace ApiMotos.Application.Agregates.Actividades.Queries.Actividades
{
    public class ActividadesDto
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public string? VendedorDisplay { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteDisplay { get; set; }
        public string Tipo { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Resultado { get; set; } = "";
        public string? Notas { get; set; }
        public DateTime? ProximaAccion { get; set; }
        public int? PedidoId { get; set; }
    }
}
