using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Vendedores
{
    /// <summary>
    /// Vendedor de la fuerza de ventas (plan §3.1). La comisión es un porcentaje fijo sobre el
    /// total USD de pedidos entregados (Etapa B). `Usuario` es el login del sitio (no un id):
    /// el bypass dev `pablo` no existe en Seg_Usuarios y así igual puede ver "lo mío".
    /// </summary>
    public class Vendedor : BaseEntity<int>
    {
        private Vendedor() : base()
        {
            Nombre = string.Empty;
        }

        private Vendedor(string pNombre, string? pTelefono, string? pEmail, string? pZona,
            decimal pComisionPorcentaje, string? pUsuario, bool pActivo)
        {
            Nombre = pNombre;
            Telefono = pTelefono;
            Email = pEmail;
            Zona = pZona;
            ComisionPorcentaje = pComisionPorcentaje;
            Usuario = pUsuario;
            Activo = pActivo;
        }

        public string Nombre { get; private set; }
        public string? Telefono { get; private set; }
        public string? Email { get; private set; }
        public string? Zona { get; private set; }
        public decimal ComisionPorcentaje { get; private set; }
        public string? Usuario { get; private set; }
        public bool Activo { get; private set; }

        private static Result Validar(string nombre, decimal comisionPorcentaje)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail("Nombre es requerido");
            if (comisionPorcentaje < 0 || comisionPorcentaje > 100)
                return Result.Fail("ComisionPorcentaje debe estar entre 0 y 100");
            return Result.Ok();
        }

        private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        public static Result<Vendedor> Crear(string nombre, string? telefono, string? email, string? zona,
            decimal comisionPorcentaje, string? usuario, bool activo)
        {
            var v = Validar(nombre, comisionPorcentaje);
            if (v.IsFailed) return Result.Fail<Vendedor>(v.Errors);
            return new Vendedor(nombre.Trim(), Limpiar(telefono), Limpiar(email), Limpiar(zona),
                comisionPorcentaje, Limpiar(usuario), activo);
        }

        public Result<Vendedor> Modificar(string pNombre, string? pTelefono, string? pEmail, string? pZona,
            decimal pComisionPorcentaje, string? pUsuario, bool pActivo)
        {
            var v = Validar(pNombre, pComisionPorcentaje);
            if (v.IsFailed) return Result.Fail<Vendedor>(v.Errors);
            Nombre = pNombre.Trim();
            Telefono = Limpiar(pTelefono);
            Email = Limpiar(pEmail);
            Zona = Limpiar(pZona);
            ComisionPorcentaje = pComisionPorcentaje;
            Usuario = Limpiar(pUsuario);
            Activo = pActivo;
            return this;
        }
    }
}
