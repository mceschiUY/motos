namespace ApiMotos.Application.Artesanal.Comercial.Comisiones
{
    /// <summary>
    /// Una fila por vendedor con lo liquidable del período (plan §4.4). La comisión se
    /// devenga con la ENTREGA: solo cuentan los pedidos entregados. Todo en USD.
    /// Etapa H.6: además de lo sellado trae la meta del mes, lo pendiente de entregar (para
    /// proyectar) y el mismo corte del mes anterior, así la pantalla compara sin recalcular.
    /// </summary>
    public class ComisionVendedorDto
    {
        public int VendedorId { get; set; }
        public string? VendedorDisplay { get; set; }
        public string? Zona { get; set; }
        public decimal ComisionPorcentaje { get; set; }
        public string Periodo { get; set; } = "";
        /// <summary>Pedidos entregados en el período.</summary>
        public int Pedidos { get; set; }
        public decimal TotalUsd { get; set; }
        /// <summary>Suma de la comisión sellada en cada pedido al entregarlo.</summary>
        public decimal ComisionUsd { get; set; }

        /// <summary>Meta del período en PC_METAS (0 si no hay).</summary>
        public decimal ObjetivoUsd { get; set; }

        /// <summary>Pedidos del período todavía no entregados ni anulados (lo que falta cobrar).</summary>
        public int PedidosPendientes { get; set; }
        /// <summary>Total USD de esos pedidos pendientes.</summary>
        public decimal PendienteEntregaUsd { get; set; }
        /// <summary>
        /// Sellada + pendiente × % de hoy: lo que cobraría si entrega todo lo que tiene abierto
        /// del mes (mismo criterio que el panel del vendedor).
        /// </summary>
        public decimal ComisionProyectadaUsd { get; set; }

        /// <summary>Período anterior (YYYY-MM) con el que se compara.</summary>
        public string PeriodoAnterior { get; set; } = "";
        public int AnteriorPedidos { get; set; }
        public decimal AnteriorTotalUsd { get; set; }
        public decimal AnteriorComisionUsd { get; set; }
        public decimal AnteriorObjetivoUsd { get; set; }
    }
}
