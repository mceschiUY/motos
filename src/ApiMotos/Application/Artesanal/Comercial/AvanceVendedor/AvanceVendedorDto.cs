namespace ApiMotos.Application.Artesanal.Comercial.AvanceVendedor
{
    /// <summary>KPIs del vendedor en un período YYYY-MM (plan §4.4 / §4.8 metas). Solo USD.</summary>
    public class AvanceVendedorDto
    {
        public int VendedorId { get; set; }
        public string Periodo { get; set; } = "";
        /// <summary>Meta del período (0 si no hay).</summary>
        public decimal ObjetivoUsd { get; set; }
        /// <summary>Vendido en el período: total de los pedidos ENTREGADOS (Etapa B).</summary>
        public decimal VendidoUsd { get; set; }
        /// <summary>Comisión devengada en el período (la sellada al entregar cada pedido).</summary>
        public decimal ComisionUsd { get; set; }
        public int Actividades { get; set; }
        public int Visitas { get; set; }
        public int ConPedido { get; set; }
        /// <summary>ConPedido / Actividades (0 si no hay actividades).</summary>
        public decimal TasaCierre { get; set; }
        public int ClientesAsignados { get; set; }
        public int ClientesSinVisitar30d { get; set; }
        /// <summary>Días del parámetro `crm.dias_sin_visita` con el que se contó ClientesSinVisitar30d.</summary>
        public int DiasSinVisitaUmbral { get; set; }
    }
}
