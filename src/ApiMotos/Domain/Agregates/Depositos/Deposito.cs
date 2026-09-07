using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Depositos
{
    public class Deposito : BaseEntity<int>
    {
        private Deposito() : base()
        {
            Codigo = string.Empty;
            Nombre = string.Empty;
        }

        private Deposito(string pCodigo, string pNombre, string? pDireccion, bool pActivo)
        {
            Codigo = pCodigo;
            Nombre = pNombre;
            Direccion = pDireccion;
            Activo = pActivo;
        }

        public string Codigo { get; private set; }
        public string Nombre { get; private set; }
        public string? Direccion { get; private set; }
        public bool Activo { get; private set; }

        public static Result<Deposito> Crear(string codigo, string nombre, string? direccion, bool activo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return Result.Fail<Deposito>("Codigo es requerido");
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Deposito>("Nombre es requerido");
            return new Deposito(codigo, nombre, direccion, activo);
        }

        public Result<Deposito> Modificar(string pCodigo, string pNombre, string? pDireccion, bool pActivo)
        {
            if (string.IsNullOrWhiteSpace(pCodigo)) return Result.Fail<Deposito>("Codigo es requerido");
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Deposito>("Nombre es requerido");
            Codigo = pCodigo;
            Nombre = pNombre;
            Direccion = pDireccion;
            Activo = pActivo;
            return this;
        }
    }
}
