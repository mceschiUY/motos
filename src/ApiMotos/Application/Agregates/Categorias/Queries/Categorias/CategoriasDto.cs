namespace ApiMotos.Application.Agregates.Categorias.Queries.Categorias
{
    public class CategoriasDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? CategoriaPadreId { get; set; }
        public bool Activo { get; set; }
    }
}
