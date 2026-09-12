namespace ApiMotos.Application.Artesanal.Catalogo.FichaSku
{
    /// <summary>
    /// Ficha de SKU (escena artesanal, solo lectura): qué es, cuánto vale, dónde está el stock,
    /// cómo se movió y cuándo se acaba. Todo en USD.
    /// </summary>
    public class FichaSkuDto
    {
        public int VarianteId { get; set; }
        public string? Sku { get; set; }
        public string? CodigoBarras { get; set; }
        public bool Activo { get; set; }
        public int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public string? ProductoCodigo { get; set; }
        public string? MarcaDisplay { get; set; }
        public string? CategoriaDisplay { get; set; }
        public int? ImagenPrincipalId { get; set; }
        public string? TallaDisplay { get; set; }
        public string? ColorDisplay { get; set; }
        public string? ColorHex { get; set; }
        public decimal PrecioListaUsd { get; set; }
        public decimal CostoEstandarUsd { get; set; }
        /// <summary>PrecioLista - CostoEstandar, en USD.</summary>
        public decimal MargenUsd { get; set; }
        /// <summary>Margen sobre el costo, en %. Cero si el costo es 0: no hay base de cálculo.</summary>
        public decimal MargenPorcentaje { get; set; }

        public List<FichaSkuDepositoDto> StockPorDeposito { get; set; } = new();
        /// <summary>Saldo del Kardex sumando todos los depósitos.</summary>
        public decimal StockTotal { get; set; }
        /// <summary>Unidades en líneas de pedidos borrador/confirmado/preparado (todavía no salieron del Kardex).</summary>
        public decimal Comprometido { get; set; }
        /// <summary>StockTotal - Comprometido.</summary>
        public decimal DisponibleNeto { get; set; }
        /// <summary>Salidas del Kardex en los últimos 30 días.</summary>
        public decimal UnidadesVendidas30d { get; set; }
        /// <summary>StockTotal / (UnidadesVendidas30d / 30). Null si no hubo ventas en 30 días.</summary>
        public decimal? CoberturaDias { get; set; }
        /// <summary>'ok' | 'bajo' (stock entre 0 y 3, exclusivo) | 'sin_stock' | 'negativo'.</summary>
        public string SemaforoStock { get; set; } = "ok";

        /// <summary>Del más nuevo al más viejo; el saldo acumulado se calculó en orden cronológico.</summary>
        public List<FichaSkuMovimientoDto> Kardex { get; set; } = new();
        /// <summary>Pedidos no entregados ni anulados que contienen el SKU.</summary>
        public List<FichaSkuPedidoDto> PedidosAbiertos { get; set; } = new();

        /// <summary>
        /// Etapa H.8: TODAS las variantes del mismo producto (esta incluida, con EsActual), con su
        /// saldo total y comprometido, ordenadas por talla y color. Responde "no tengo 42, ¿qué
        /// talle sí tengo?" sin salir de la ficha.
        /// </summary>
        public List<FichaSkuHermanoDto> Hermanos { get; set; } = new();
    }

    public class FichaSkuHermanoDto
    {
        public int VarianteId { get; set; }
        public string? Sku { get; set; }
        public bool Activo { get; set; }
        public bool EsActual { get; set; }
        public int? TallaId { get; set; }
        public string? TallaDisplay { get; set; }
        public int TallaOrden { get; set; }
        public int? ColorId { get; set; }
        public string? ColorDisplay { get; set; }
        public string? ColorHex { get; set; }
        public decimal PrecioListaUsd { get; set; }
        /// <summary>Saldo del Kardex sumando todos los depósitos.</summary>
        public decimal Saldo { get; set; }
        /// <summary>Unidades en pedidos borrador/confirmado/preparado.</summary>
        public decimal Comprometido { get; set; }
        /// <summary>'ok' | 'bajo' | 'sin_stock' | 'negativo', misma regla que SemaforoStock.</summary>
        public string Semaforo { get; set; } = "ok";
    }

    public class FichaSkuDepositoDto
    {
        public int DepositoId { get; set; }
        public string? DepositoNombre { get; set; }
        public decimal Saldo { get; set; }
    }

    public class FichaSkuMovimientoDto
    {
        public int MovimientoId { get; set; }
        public DateTime Fecha { get; set; }
        public string? Tipo { get; set; }
        public decimal Cantidad { get; set; }
        /// <summary>Efecto sobre el stock TOTAL, con signo (una transferencia entre depósitos vale 0).</summary>
        public decimal Delta { get; set; }
        /// <summary>Saldo total (todos los depósitos) después de este movimiento.</summary>
        public decimal SaldoAcumulado { get; set; }
        public int DepositoId { get; set; }
        public string? DepositoNombre { get; set; }
        public int? DepositoDestinoId { get; set; }
        public string? DepositoDestinoNombre { get; set; }
        public string? DocumentoOrigen { get; set; }
        /// <summary>Pedido cuyo Numero coincide con DocumentoOrigen, si lo hay.</summary>
        public int? PedidoId { get; set; }
        public string? Motivo { get; set; }
        public decimal? CostoUnitario { get; set; }
        public string? Usuario { get; set; }
    }

    public class FichaSkuPedidoDto
    {
        public int PedidoId { get; set; }
        public string? Numero { get; set; }
        public string? Estado { get; set; }
        public string? ClienteNombre { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime Fecha { get; set; }
    }
}
