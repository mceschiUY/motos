namespace ApiMotos.Application.Artesanal.Comercial.Comisiones
{
    /// <summary>
    /// Una fila por vendedor con lo liquidable del período (plan §4.4). La comisión se
    /// devenga con la ENTREGA: solo cuentan los pedidos entregados. Todo en USD.
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
    }
}
