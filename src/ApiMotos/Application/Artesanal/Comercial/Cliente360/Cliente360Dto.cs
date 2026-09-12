namespace ApiMotos.Application.Artesanal.Comercial.Cliente360
{
    /// <summary>
    /// Escena "Cliente 360": lo que el vendedor mira antes de entrar a la tienda. Cabecera con
    /// todos los datos del cliente, salud de la relación (semáforo), línea de tiempo unificada
    /// (actividades + pedidos + envíos) y los productos que más compra.
    /// </summary>
    public class Cliente360Dto
    {
        // ─── Cabecera ───
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Tipo { get; set; }
        public string? Ciudad { get; set; }
        public string? DireccionEntrega { get; set; }
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Notas { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int? VendedorId { get; set; }
        public string? VendedorNombre { get; set; }
        public string? VendedorTelefono { get; set; }

        // ─── Salud de la relación ───
        /// <summary>Días desde la última actividad de cualquier tipo; null si nunca se lo contactó.</summary>
        public int? DiasSinVisita { get; set; }
        public DateTime? UltimaActividadFecha { get; set; }
        public string? UltimaActividadTipo { get; set; }
        public string? UltimaActividadResultado { get; set; }
        /// <summary>Mínima ProximaAccion pendiente (>= hoy) entre las actividades del cliente.</summary>
        public DateTime? ProximaAccion { get; set; }
        /// <summary>Pedidos del año calendario en curso (excluye anulados).</summary>
        public int PedidosAnio { get; set; }
        public decimal TotalAnioUsd { get; set; }
        public decimal TicketPromedioUsd { get; set; }
        public Cliente360PedidoDto? UltimoPedido { get; set; }
        /// <summary>Pedidos en borrador, confirmado, preparado o despachado.</summary>
        public int PedidosAbiertos { get; set; }
        /// <summary>Envíos del cliente que no están entregados ni anulados.</summary>
        public int EnviosEnCurso { get; set; }
        /// <summary>verde (visita ≤ 15 días) · amarillo (16–30 o sin pedidos en 90 días) · rojo (> 30 o nunca visitado).</summary>
        public string Semaforo { get; set; } = "rojo";
        public string MotivoSemaforo { get; set; } = "";

        public List<Cliente360TimelineItemDto> Timeline { get; set; } = new();
        public List<Cliente360TopProductoDto> TopProductos { get; set; } = new();
    }

    public class Cliente360PedidoDto
    {
        public int Id { get; set; }
        public string? Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string? Estado { get; set; }
        public decimal TotalUsd { get; set; }
    }

    /// <summary>Un evento de la línea de tiempo. `Tipo` = actividad | pedido | envio.</summary>
    public class Cliente360TimelineItemDto
    {
        public string Tipo { get; set; } = "";
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Titulo { get; set; } = "";
        public string? Detalle { get; set; }
        /// <summary>Valor crudo del enum para la pill (resultado de la actividad, estado del pedido/envío).</summary>
        public string? Estado { get; set; }
        public string Ruta { get; set; } = "";
    }

    public class Cliente360TopProductoDto
    {
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public decimal Unidades { get; set; }
        public decimal TotalUsd { get; set; }
    }
}
