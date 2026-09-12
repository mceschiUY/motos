namespace ApiMotos.Application.Artesanal.Catalogo.FichaProducto
{
    /// <summary>Ficha comercial de un producto: la hoja que se le muestra (o imprime) al cliente.</summary>
    public class FichaProductoDto
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public int MarcaId { get; set; }
        public string? MarcaDisplay { get; set; }
        public int CategoriaId { get; set; }
        public string? CategoriaDisplay { get; set; }
        public string? Genero { get; set; }
        public string? Temporada { get; set; }
        public string? Material { get; set; }
        public int? PesoGramos { get; set; }
        public string? Descripcion { get; set; }
        public string? FichaTecnica { get; set; }
        public string? TipoCasco { get; set; }
        public string? Homologacion { get; set; }
        public bool? HomologacionVigente { get; set; }
        public DateTime? FechaVencHomologacion { get; set; }
        public bool Destacado { get; set; }
        public bool Novedad { get; set; }
        public int? ImagenPrincipalId { get; set; }

        /// <summary>Deposito sobre el que se calcularon las existencias (null = todos).</summary>
        public int? DepositoId { get; set; }
        public string? DepositoDisplay { get; set; }

        public decimal PrecioDesdeUsd { get; set; }
        public decimal UnidadesTotales { get; set; }
        /// <summary>Margen promedio de los SKU con costo cargado, en %.</summary>
        public decimal MargenPromedioPorcentaje { get; set; }
        /// <summary>Umbral de stock bajo (Configuración `stock.umbral_bajo`), para pintar las celdas (revisión 2026-09-12).</summary>
        public decimal UmbralStockBajo { get; set; }
        /// <summary>SKU activos con saldo por debajo del umbral.</summary>
        public int SkusEnAlerta { get; set; }

        public List<FichaProductoVarianteDto> Variantes { get; set; } = new();
    }

    /// <summary>Una celda de la matriz talla x color.</summary>
    public class FichaProductoVarianteDto
    {
        public int VarianteId { get; set; }
        public string? Sku { get; set; }
        public int? TallaId { get; set; }
        public string? TallaDisplay { get; set; }
        public int TallaOrden { get; set; }
        public int? ColorId { get; set; }
        public string? ColorDisplay { get; set; }
        public string? ColorHex { get; set; }
        public decimal PrecioListaUsd { get; set; }
        public decimal CostoEstandarUsd { get; set; }
        /// <summary>PrecioLista - CostoEstandar, en USD.</summary>
        public decimal MargenUsd { get; set; }
        /// <summary>Margen sobre el costo, en %. Cero si el costo es 0: no hay base de calculo.</summary>
        public decimal MargenPorcentaje { get; set; }
        public decimal Disponible { get; set; }
        public bool Activo { get; set; }
        /// <summary>Unidades en pedidos borrador/confirmado/preparado (revisión 2026-09-12).</summary>
        public decimal Comprometido { get; set; }
        public decimal Vendidas30d { get; set; }
        /// <summary>Días que dura el saldo al ritmo de 30 días; null sin ventas o sin stock.</summary>
        public decimal? CoberturaDias { get; set; }
    }
}
