using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Colores
{
    public class Color : BaseEntity<int>
    {
        private Color() : base()
        {
            Nombre = string.Empty;
            CodigoHex = string.Empty;
        }

        private Color(string pNombre, string pCodigoHex, bool pActivo)
        {
            Nombre = pNombre;
            CodigoHex = pCodigoHex;
            Activo = pActivo;
        }

        public string Nombre { get; private set; }
        public string CodigoHex { get; private set; }
        public bool Activo { get; private set; }

        public static Result<Color> Crear(string nombre, string codigoHex, bool activo)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Color>("Nombre es requerido");
            return new Color(nombre, codigoHex ?? string.Empty, activo);
        }

        public Result<Color> Modificar(string pNombre, string pCodigoHex, bool pActivo)
        {
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Color>("Nombre es requerido");
            Nombre = pNombre;
            CodigoHex = pCodigoHex ?? string.Empty;
            Activo = pActivo;
            return this;
        }
    }
}
