namespace ApiMotos.Application.Artesanal.Catalogo.Existencias
{
    public class ExistenciasDto
    {
        /// <summary>Umbral de stock bajo (Configuración `stock.umbral_bajo`, default 3).</summary>
        public decimal UmbralStockBajo { get; set; }
        /// <summary>Depósito por el que se filtró (null = todos).</summary>
        public int? DepositoId { get; set; }
        public List<ExistenciasDepositoDto> Depositos { get; set; } = new();
        public List<ExistenciaFilaDto> Filas { get; set; } = new();
    }

    public class ExistenciasDepositoDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
    }

    /// <summary>Una fila por SKU (variante activa de producto activo), aunque no tenga movimientos.</summary>
    public class ExistenciaFilaDto
    {
        public int VarianteId { get; set; }
        public string? Sku { get; set; }
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public string? ProductoCodigo { get; set; }
        public string? Marca { get; set; }
        public int? CategoriaId { get; set; }
        public string? Categoria { get; set; }
        /// <summary>Categoría de primer nivel (para agrupar: "Cascos" aunque el SKU sea "Integral").</summary>
        public string? CategoriaRaiz { get; set; }
        public string? Talla { get; set; }
        public string? Color { get; set; }
        public string? ColorHex { get; set; }
        public decimal PrecioListaUsd { get; set; }
        /// <summary>Saldo en el depósito elegido, o total de todos.</summary>
        public decimal Saldo { get; set; }
        public List<ExistenciaSaldoDepositoDto> PorDeposito { get; set; } = new();
        /// <summary>Unidades en pedidos borrador / confirmado / preparado (todavía no salieron del Kardex).</summary>
        public decimal Comprometido { get; set; }
        public decimal Vendidas30d { get; set; }
        /// <summary>Días que dura el saldo al ritmo de los últimos 30 días; null sin ventas.</summary>
        public decimal? CoberturaDias { get; set; }
        /// <summary>negativo · sin_stock · bajo · ok</summary>
        public string Semaforo { get; set; } = "ok";
    }

    public class ExistenciaSaldoDepositoDto
    {
        public int DepositoId { get; set; }
        public string? Nombre { get; set; }
        public decimal Saldo { get; set; }
    }
}
