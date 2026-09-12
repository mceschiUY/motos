namespace ApiMotos.Application.Artesanal.Logistica.SeguimientoEnvio
{
    /// <summary>
    /// Seguimiento de un envío estilo courier: cabecera, línea de estados con SLA por etapa,
    /// pedido origen con líneas y bitácora de observaciones. En modo público los campos
    /// sensibles (teléfono, usuario, precios, totales) viajan en null.
    /// </summary>
    public class SeguimientoEnvioDto
    {
        public int EnvioId { get; set; }
        public string CodigoRastreo { get; set; } = string.Empty;
        /// <summary>Valor persistido: recibido | facturado | despachado | entregado | anulado.</summary>
        public string Estado { get; set; } = string.Empty;
        /// <summary>Etiqueta que se muestra ('facturado' se lee "Confirmado", descriptor de Envio).</summary>
        public string EstadoLabel { get; set; } = string.Empty;
        public string? MotivoAnulacion { get; set; }
        public bool Publico { get; set; }

        public SeguimientoClienteDto Cliente { get; set; } = new();
        public SeguimientoAgenciaDto Agencia { get; set; } = new();

        /// <summary>Los 4 hitos en orden: recibido, confirmado, despachado, entregado.</summary>
        public List<SeguimientoEtapaDto> Etapas { get; set; } = new();
        /// <summary>estadoSla de la etapa en curso; null si el envío está entregado o anulado.</summary>
        public string? SlaGlobal { get; set; }

        public SeguimientoPedidoDto? Pedido { get; set; }
        /// <summary>De la más nueva a la más vieja.</summary>
        public List<SeguimientoObservacionDto> Observaciones { get; set; } = new();
    }

    public class SeguimientoClienteDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Ciudad { get; set; }
        public string? DireccionEntrega { get; set; }
        /// <summary>Solo en modo interno.</summary>
        public string? Telefono { get; set; }
    }

    public class SeguimientoAgenciaDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
    }

    public class SeguimientoEtapaDto
    {
        /// <summary>recibido | confirmado | despachado | entregado</summary>
        public string Clave { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        /// <summary>Fecha en que se alcanzó el hito. Null si todavía no se alcanzó.</summary>
        public DateTime? Fecha { get; set; }
        public bool Cumplida { get; set; }
        /// <summary>El hito donde está parado el envío hoy (último cumplido).</summary>
        public bool Actual { get; set; }
        /// <summary>
        /// Etapa en curso: días desde que empezó hasta hoy. Etapa cumplida: días que tardó
        /// (hito anterior → este hito). Null para el hito 'recibido' y para los no alcanzados.
        /// </summary>
        public int? DiasTranscurridos { get; set; }
        /// <summary>Parámetros SLA de la etapa que lleva a este hito (PC_PARAMETROSLAS). Null si no aplica.</summary>
        public int? UmbralDias { get; set; }
        public int? LimiteDias { get; set; }
        /// <summary>ok | advertencia | vencido | null (sin SLA: 'recibido', hitos no alcanzados, anulado).</summary>
        public string? EstadoSla { get; set; }
    }

    public class SeguimientoPedidoDto
    {
        public int Id { get; set; }
        public string? Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string? Estado { get; set; }
        public string? VendedorNombre { get; set; }
        /// <summary>Solo en modo interno.</summary>
        public decimal? TotalUsd { get; set; }
        public List<SeguimientoLineaDto> Lineas { get; set; } = new();
    }

    public class SeguimientoLineaDto
    {
        public int VarianteId { get; set; }
        public string? Sku { get; set; }
        public string? ProductoNombre { get; set; }
        public string? Talla { get; set; }
        public string? Color { get; set; }
        public decimal Cantidad { get; set; }
        /// <summary>Solo en modo interno.</summary>
        public decimal? PrecioUnitarioUsd { get; set; }
        /// <summary>Solo en modo interno.</summary>
        public decimal? SubtotalUsd { get; set; }
    }

    public class SeguimientoObservacionDto
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        /// <summary>Solo en modo interno.</summary>
        public string? Usuario { get; set; }
        public string? Texto { get; set; }
    }
}
