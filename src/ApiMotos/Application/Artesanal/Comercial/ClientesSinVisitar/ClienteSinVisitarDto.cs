namespace ApiMotos.Application.Artesanal.Comercial.ClientesSinVisitar
{
    /// <summary>Alerta "cliente sin visitar" (plan §4.8): cliente con vendedor asignado y sin actividad en N días.</summary>
    public class ClienteSinVisitarDto
    {
        public int ClienteId { get; set; }
        public string? ClienteDisplay { get; set; }
        public string? Ciudad { get; set; }
        public int VendedorId { get; set; }
        public string? VendedorDisplay { get; set; }
        /// <summary>Fecha de la última actividad registrada; null si nunca se lo contactó.</summary>
        public DateTime? UltimaActividad { get; set; }
        /// <summary>Días desde la última actividad; null si nunca se lo contactó.</summary>
        public int? DiasSinVisita { get; set; }
    }
}
