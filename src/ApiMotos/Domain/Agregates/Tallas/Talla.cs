using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Tallas
{
    public class Talla : BaseEntity<int>
    {
        private Talla() : base()
        {
            Nombre = string.Empty;
            Tipo = "alfabetica";
        }

        private Talla(string pNombre, string pTipo, int pOrden, bool pActivo)
        {
            Nombre = pNombre;
            Tipo = pTipo;
            Orden = pOrden;
            Activo = pActivo;
        }

        public string Nombre { get; private set; }
        public string Tipo { get; private set; }
        public int Orden { get; private set; }
        public bool Activo { get; private set; }

        public static Result<Talla> Crear(string nombre, string tipo, int orden, bool activo)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Talla>("Nombre es requerido");
            return new Talla(nombre, string.IsNullOrWhiteSpace(tipo) ? "alfabetica" : tipo, orden, activo);
        }

        public Result<Talla> Modificar(string pNombre, string pTipo, int pOrden, bool pActivo)
        {
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Talla>("Nombre es requerido");
            Nombre = pNombre;
            Tipo = string.IsNullOrWhiteSpace(pTipo) ? "alfabetica" : pTipo;
            Orden = pOrden;
            Activo = pActivo;
            return this;
        }
    }
}
