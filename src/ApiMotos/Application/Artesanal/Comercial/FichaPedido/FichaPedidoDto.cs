namespace ApiMotos.Application.Artesanal.Comercial.FichaPedido
{
    public class FichaPedidoDto
    {
        public int Id { get; set; }
        public string? Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "";
        public decimal TotalUsd { get; set; }
        public decimal ComisionUsd { get; set; }
        /// <summary>% del vendedor: sirve para mostrar la comisión estimada antes de entregar.</summary>
        public decimal ComisionPorcentaje { get; set; }
        public string? Observaciones { get; set; }
        public string? MotivoAnulacion { get; set; }

        public int ClienteId { get; set; }
        public string? ClienteNombre { get; set; }
        public string? ClienteTipo { get; set; }
        public string? ClienteCiudad { get; set; }
        public string? ClienteDireccion { get; set; }
        public string? ClienteTelefono { get; set; }
        public int VendedorId { get; set; }
        public string? VendedorNombre { get; set; }
        public int DepositoId { get; set; }
        public string? DepositoNombre { get; set; }
        public int? AgenciaId { get; set; }
        public string? AgenciaNombre { get; set; }

        public int? EnvioId { get; set; }
        public string? EnvioCodigo { get; set; }
        public string? EnvioEstado { get; set; }
        public DateTime? EnvioFechaEnvio { get; set; }
        public DateTime? EnvioFechaEntrega { get; set; }

        public List<FichaPedidoLineaDto> Lineas { get; set; } = new();
        public decimal Unidades { get; set; }
        /// <summary>Líneas que piden más de lo que hay en el depósito (aviso de faltante).</summary>
        public int LineasConFaltante { get; set; }
    }

    public class FichaPedidoLineaDto
    {
        public int Id { get; set; }
        public int VarianteId { get; set; }
        public string? Sku { get; set; }
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public string? Marca { get; set; }
        public int? ImagenPrincipalId { get; set; }
        public string? Talla { get; set; }
        public string? Color { get; set; }
        public string? ColorHex { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitarioUsd { get; set; }
        public decimal SubtotalUsd { get; set; }
        /// <summary>Precio de lista actual de la variante, para ver si se vendió con descuento.</summary>
        public decimal PrecioListaUsd { get; set; }
        /// <summary>Saldo actual en el depósito del pedido (null si el pedido ya salió: no aplica).</summary>
        public decimal? DisponibleDeposito { get; set; }
    }
}
