namespace ApiMotos.Application.Agregates.Productos.Queries.Productos
{
    public class ProductosDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int MarcaId { get; set; }
        public string? MarcaDisplay { get; set; }
        public int CategoriaId { get; set; }
        public string? CategoriaDisplay { get; set; }
        public string? Descripcion { get; set; }
        public string Genero { get; set; }
        public string? Temporada { get; set; }
        public string? Material { get; set; }
        public int? PesoGramos { get; set; }
        public string? TipoCasco { get; set; }
        public string? Homologacion { get; set; }
        public bool? HomologacionVigente { get; set; }
        public DateTime? FechaVencHomologacion { get; set; }
        public bool Activo { get; set; }
    }
}
