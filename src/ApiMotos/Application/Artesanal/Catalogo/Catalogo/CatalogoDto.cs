namespace ApiMotos.Application.Artesanal.Catalogo.Catalogo
{
    /// <summary>Una card del catálogo navegable.</summary>
    public class CatalogoItemDto
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public int MarcaId { get; set; }
        public string? MarcaDisplay { get; set; }
        public int CategoriaId { get; set; }
        public string? CategoriaDisplay { get; set; }
        public string? Genero { get; set; }
        public bool Destacado { get; set; }
        public bool Novedad { get; set; }
        /// <summary>Id en PC_DOCUMENTOS de la portada. El front la pide en base64 y arma un data-URL.</summary>
        public int? ImagenPrincipalId { get; set; }
        /// <summary>MIN(PrecioLista) de las variantes activas. 0 si el producto todavía no tiene SKU.</summary>
        public decimal PrecioDesdeUsd { get; set; }
        /// <summary>Cantidad de SKU activos (celdas de la matriz talla x color).</summary>
        public int Skus { get; set; }
        /// <summary>Unidades en stock sumando todos los depósitos.</summary>
        public decimal Unidades { get; set; }
        /// <summary>SKU activos con saldo positivo (revisión de escenas 2026-09-12).</summary>
        public int SkusConStock { get; set; }
        /// <summary>SKU activos con saldo por debajo del umbral de stock bajo (incluye sin stock y negativo).</summary>
        public int SkusEnAlerta { get; set; }
        /// <summary>sin_stock (ningún SKU con saldo) · bajo (algún SKU en alerta) · ok.</summary>
        public string Semaforo { get; set; } = "ok";
        /// <summary>Umbral usado (Configuración `stock.umbral_bajo`).</summary>
        public decimal UmbralStockBajo { get; set; }
    }
}
