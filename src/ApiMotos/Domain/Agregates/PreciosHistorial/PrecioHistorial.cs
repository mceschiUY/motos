using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.PreciosHistorial
{
    /// <summary>
    /// Cambio de precio o costo de una Variante (plan §3.8 / §4.8, Etapa C).
    /// Entidad de solo escritura: la llena <c>VarianteHooks</c> cuando el PUT de una variante
    /// cambia <c>PrecioLista</c> o <c>CostoEstandar</c>. No tiene CRUD ni pantalla de alta:
    /// se lee como timeline en la ficha de Variante y alimenta el margen de la ficha comercial.
    /// </summary>
    public class PrecioHistorial : BaseEntity<int>
    {
        /// <summary>Campos que se historian. El valor persiste en minúsculas (pill del front).</summary>
        public static readonly string[] CamposValidos = { "precio", "costo" };

        private PrecioHistorial() : base()
        {
            Campo = string.Empty;
        }

        private PrecioHistorial(int pVarianteId, string pCampo, decimal pValorAnterior, decimal pValorNuevo,
            DateTime pFecha, string? pUsuario)
        {
            VarianteId = pVarianteId;
            Campo = pCampo;
            ValorAnterior = pValorAnterior;
            ValorNuevo = pValorNuevo;
            Fecha = pFecha;
            Usuario = pUsuario;
        }

        public int VarianteId { get; private set; }
        /// <summary>`precio` (PrecioLista) o `costo` (CostoEstandar).</summary>
        public string Campo { get; private set; }
        public decimal ValorAnterior { get; private set; }
        public decimal ValorNuevo { get; private set; }
        public DateTime Fecha { get; private set; }
        /// <summary>Login del usuario que lo cambió (claim Name del JWT); null si no hay sesión.</summary>
        public string? Usuario { get; private set; }

        public static Result<PrecioHistorial> Crear(int varianteId, string campo, decimal valorAnterior,
            decimal valorNuevo, DateTime fecha, string? usuario)
        {
            if (varianteId <= 0) return Result.Fail<PrecioHistorial>("Variante es requerida");
            var c = (campo ?? string.Empty).Trim().ToLowerInvariant();
            if (!CamposValidos.Contains(c))
                return Result.Fail<PrecioHistorial>("Campo inválido: debe ser 'precio' o 'costo'");
            if (valorAnterior == valorNuevo)
                return Result.Fail<PrecioHistorial>("No se historia un cambio que no cambió nada");
            var u = string.IsNullOrWhiteSpace(usuario) ? null : usuario.Trim();
            return new PrecioHistorial(varianteId, c, valorAnterior, valorNuevo, fecha, u);
        }
    }
}
