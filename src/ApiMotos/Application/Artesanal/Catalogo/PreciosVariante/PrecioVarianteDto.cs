namespace ApiMotos.Application.Artesanal.Catalogo.PreciosVariante
{
    public class PrecioVarianteDto
    {
        public int Id { get; set; }
        public int VarianteId { get; set; }
        /// <summary>`precio` (PrecioLista) o `costo` (CostoEstandar).</summary>
        public string Campo { get; set; } = "";
        public decimal ValorAnterior { get; set; }
        public decimal ValorNuevo { get; set; }
        /// <summary>Diferencia con signo, en USD.</summary>
        public decimal DiferenciaUsd { get; set; }
        /// <summary>Variación porcentual sobre el valor anterior (0 si el anterior era 0).</summary>
        public decimal DiferenciaPorcentaje { get; set; }
        public DateTime Fecha { get; set; }
        public string? Usuario { get; set; }
    }
}
