using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Categorias
{
    public class Categoria : BaseEntity<int>
    {
        private Categoria() : base()
        {
            Nombre = string.Empty;
        }

        private Categoria(string pNombre, int? pCategoriaPadreId, bool pActivo)
        {
            Nombre = pNombre;
            CategoriaPadreId = pCategoriaPadreId;
            Activo = pActivo;
        }

        public string Nombre { get; private set; }
        public int? CategoriaPadreId { get; private set; }
        public bool Activo { get; private set; }

        public static Result<Categoria> Crear(string nombre, int? categoriaPadreId, bool activo)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Categoria>("Nombre es requerido");
            return new Categoria(nombre, categoriaPadreId, activo);
        }

        public Result<Categoria> Modificar(string pNombre, int? pCategoriaPadreId, bool pActivo)
        {
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Categoria>("Nombre es requerido");
            if (pCategoriaPadreId.HasValue && pCategoriaPadreId.Value == Id)
                return Result.Fail<Categoria>("Una categoría no puede ser su propia categoría padre");
            Nombre = pNombre;
            CategoriaPadreId = pCategoriaPadreId;
            Activo = pActivo;
            return this;
        }
    }
}
