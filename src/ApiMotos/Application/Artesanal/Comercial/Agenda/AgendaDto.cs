namespace ApiMotos.Application.Artesanal.Comercial.Agenda
{
    /// <summary>
    /// Ítem de la agenda del vendedor (plan §4.2 / §4.8 "ruta del día"). Lista plana ordenada
    /// por fecha: `planificada` = actividad con ProximaAccion en el rango (Fecha = ProximaAccion);
    /// `realizada` = actividad cuya Fecha cae en el rango. Latitud/Longitud del cliente para el mini-mapa.
    /// </summary>
    public class AgendaDto
    {
        /// <summary>Solo fecha (medianoche): la de la próxima acción o la de la actividad.</summary>
        public DateTime Fecha { get; set; }
        /// <summary>'planificada' | 'realizada'</summary>
        public string Tipo { get; set; } = "";
        public int ActividadId { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteDisplay { get; set; }
        public string? Ciudad { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int VendedorId { get; set; }
        public string? VendedorDisplay { get; set; }
        /// <summary>visita | llamada | whatsapp | email</summary>
        public string TipoActividad { get; set; } = "";
        /// <summary>pedido | sin_pedido | reprogramar | sin_contacto</summary>
        public string Resultado { get; set; } = "";
        public string? Notas { get; set; }
    }
}
