namespace ApiMotos.Application.Agregates.Observaciones.Queries.Observaciones
{
    public class ObservacionesDto
    {
        public int Id { get; set; }
        public string Texto { get; set; }
        public DateTime FechaHora { get; set; }
        public string Usuario { get; set; }
        public int EnvioId { get; set; }
        public string? EnvioDisplay { get; set; }
    }
}
