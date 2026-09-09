namespace ApiMotos.Application.Agregates.Metas.Queries.Metas
{
    public class MetasDto
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }
        public string? VendedorDisplay { get; set; }
        public string Periodo { get; set; } = "";
        public decimal ObjetivoUsd { get; set; }
    }
}
