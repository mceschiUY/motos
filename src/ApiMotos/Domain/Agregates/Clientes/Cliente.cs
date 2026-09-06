using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Clientes
{
    public class Cliente : BaseEntity<int>
    {
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

        public static Result<Cliente> Crear(string nombre, string telefono, string direccionEntrega)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Cliente>("Nombre es requerido");
            return new Cliente(nombre, telefono, direccionEntrega);
        }

        public Result<Cliente> Modificar(string pNombre, string pTelefono, string pDireccionEntrega)
        {
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Cliente>("Nombre es requerido");
            Nombre = pNombre;
            Telefono = pTelefono;
            DireccionEntrega = pDireccionEntrega;
            return this;
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
