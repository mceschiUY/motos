using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Agencias
{
    public class Agencia : BaseEntity<int>
    {
        private Agencia() : base()
        {
            Nombre = string.Empty;
        }

        private Agencia(string pNombre)
        {
            Nombre = pNombre;
        }

        public string Nombre { get; private set; }

        public static Result<Agencia> Crear(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Agencia>("Nombre es requerido");
            return new Agencia(nombre);
        }

        public Result<Agencia> Modificar(string pNombre)
        {
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Agencia>("Nombre es requerido");
            Nombre = pNombre;
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
        /// Valida si se puede crear una instancia de Agencia
        /// </summary>
        public static Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeCrearAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede modificar esta instancia de Agencia
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeModificarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede eliminar esta instancia de Agencia
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeEliminarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }


    }
}
