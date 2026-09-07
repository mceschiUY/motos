using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Marcas
{
    public class Marca : BaseEntity<int>
    {
        private Marca() : base()
        {
            Nombre = string.Empty;
            Pais = string.Empty;
        }

        private Marca(string pNombre, string pPais, bool pActivo)
        {
            Nombre = pNombre;
            Pais = pPais;
            Activo = pActivo;
        }

        public string Nombre { get; private set; }
        public string Pais { get; private set; }
        public bool Activo { get; private set; }

        public static Result<Marca> Crear(string nombre, string pais, bool activo)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Marca>("Nombre es requerido");
            return new Marca(nombre, pais ?? string.Empty, activo);
        }

        public Result<Marca> Modificar(string pNombre, string pPais, bool pActivo)
        {
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Marca>("Nombre es requerido");
            Nombre = pNombre;
            Pais = pPais ?? string.Empty;
            Activo = pActivo;
            return this;
        }
    }
}
