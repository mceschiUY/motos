using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Clientes
{
    public class Cliente : BaseEntity<int>
    {
        /// <summary>Etapa A (plan §3.2): tipo comercial del cliente, renderizado como enum-pill.</summary>
        public static readonly string[] TiposValidos = { "tienda", "distribuidor", "online", "particular" };

        private Cliente() : base()
        {
            Nombre = string.Empty;
            Telefono = string.Empty;
            DireccionEntrega = string.Empty;
        }

        private Cliente(string pNombre, string pTelefono, string pDireccionEntrega)
        {
            Nombre = pNombre;
            Telefono = pTelefono;
            DireccionEntrega = pDireccionEntrega;
        }

        public string Nombre { get; private set; }
        public string Telefono { get; private set; }
        public string DireccionEntrega { get; private set; }

        // ─── Etapa A (plan §3.2 / §3.8): datos comerciales, TODOS opcionales. Columnas nuevas
        //     nullable en PC_CLIENTES: Envío y Observación siguen usando el cliente igual que antes.
        public string? Tipo { get; private set; }
        public string? Ciudad { get; private set; }
        public string? Contacto { get; private set; }
        public string? Email { get; private set; }
        public int? VendedorId { get; private set; }
        public string? Notas { get; private set; }
        public decimal? Latitud { get; private set; }
        public decimal? Longitud { get; private set; }

        private static Result ValidarComercial(string? tipo, decimal? latitud, decimal? longitud)
        {
            if (!string.IsNullOrWhiteSpace(tipo) && Array.IndexOf(TiposValidos, tipo) < 0)
                return Result.Fail("Tipo inválido (tienda, distribuidor, online o particular)");
            if (latitud is < -90 or > 90) return Result.Fail("Latitud debe estar entre -90 y 90");
            if (longitud is < -180 or > 180) return Result.Fail("Longitud debe estar entre -180 y 180");
            return Result.Ok();
        }

        private static string? Limpiar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        public static Result<Cliente> Crear(string nombre, string telefono, string direccionEntrega,
            string? tipo = null, string? ciudad = null, string? contacto = null, string? email = null,
            int? vendedorId = null, string? notas = null, decimal? latitud = null, decimal? longitud = null)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Cliente>("Nombre es requerido");
            var v = ValidarComercial(tipo, latitud, longitud);
            if (v.IsFailed) return Result.Fail<Cliente>(v.Errors);
            var cliente = new Cliente(nombre, telefono, direccionEntrega);
            cliente.AsignarComercial(tipo, ciudad, contacto, email, vendedorId, notas, latitud, longitud);
            return cliente;
        }

        public Result<Cliente> Modificar(string pNombre, string pTelefono, string pDireccionEntrega,
            string? pTipo = null, string? pCiudad = null, string? pContacto = null, string? pEmail = null,
            int? pVendedorId = null, string? pNotas = null, decimal? pLatitud = null, decimal? pLongitud = null)
        {
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Cliente>("Nombre es requerido");
            var v = ValidarComercial(pTipo, pLatitud, pLongitud);
            if (v.IsFailed) return Result.Fail<Cliente>(v.Errors);
            Nombre = pNombre;
            Telefono = pTelefono;
            DireccionEntrega = pDireccionEntrega;
            AsignarComercial(pTipo, pCiudad, pContacto, pEmail, pVendedorId, pNotas, pLatitud, pLongitud);
            return this;
        }

        private void AsignarComercial(string? tipo, string? ciudad, string? contacto, string? email,
            int? vendedorId, string? notas, decimal? latitud, decimal? longitud)
        {
            Tipo = Limpiar(tipo);
            Ciudad = Limpiar(ciudad);
            Contacto = Limpiar(contacto);
            Email = Limpiar(email);
            VendedorId = vendedorId is > 0 ? vendedorId : null;
            Notas = Limpiar(notas);
            Latitud = latitud;
            Longitud = longitud;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // SISTEMA DE VALIDACIÓN UNIVERSAL - Métodos de validación de capabilities
        // ═══════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Valida si se puede ejecutar una acción específica
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeEjecutarAsync(string actionKey, CancellationToken ct = default)
        {
            return actionKey.ToLower() switch
            {
                "crear" or "create" => PuedeCrearAsync(ct),
                "modificar" or "update" => PuedeModificarAsync(ct),
                "eliminar" or "delete" => PuedeEliminarAsync(ct),
                _ => Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success())
            };
        }

        /// <summary>
        /// Valida si se puede crear una instancia de Cliente
        /// </summary>
        public static Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeCrearAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede modificar esta instancia de Cliente
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeModificarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede eliminar esta instancia de Cliente
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeEliminarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }


    }
}
