namespace ApiMotos.Application.Agregates.Pedidos.Queries.Pedidos
{
    public class PedidosDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = "";
        public DateTime Fecha { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteDisplay { get; set; }
        public int VendedorId { get; set; }
        public string? VendedorDisplay { get; set; }
        public int DepositoId { get; set; }
        public string? DepositoDisplay { get; set; }
        public int? AgenciaId { get; set; }
        public string? AgenciaDisplay { get; set; }
        public string Estado { get; set; } = "";
        public decimal TotalUsd { get; set; }
        public decimal ComisionUsd { get; set; }
        public string? Observaciones { get; set; }
        public int? EnvioId { get; set; }
        public string? EnvioDisplay { get; set; }
        public string? MotivoAnulacion { get; set; }
        /// <summary>Cuántas líneas tiene: la lista lo muestra sin pedir el detalle.</summary>
        public int Lineas { get; set; }
    }
}
