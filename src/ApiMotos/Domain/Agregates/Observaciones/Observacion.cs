using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Observaciones
{
    public class Observacion : BaseEntity<int>
    {
        private Observacion() : base()
        {
            Texto = string.Empty;
            FechaHora = Clock.Current.Now;
            Usuario = string.Empty;
            EnvioId = 0;
        }

        private Observacion(string pTexto, DateTime pFechaHora, string pUsuario, int pEnvioId)
        {
            Texto = pTexto;
            FechaHora = pFechaHora;
            Usuario = pUsuario;
            EnvioId = pEnvioId;
        }

        public string Texto { get; private set; }
        public DateTime FechaHora { get; private set; }
        public string Usuario { get; private set; }
        public int EnvioId { get; private set; }

        public static Result<Observacion> Crear(string texto, DateTime fechaHora, string usuario, int envioId)
        {
            if (string.IsNullOrWhiteSpace(texto)) return Result.Fail<Observacion>("Texto es requerido");
            if (string.IsNullOrWhiteSpace(usuario)) return Result.Fail<Observacion>("Usuario es requerido");
            return new Observacion(texto, fechaHora, usuario, envioId);
        }

        public Result<Observacion> Modificar(string pTexto, DateTime pFechaHora, string pUsuario, int pEnvioId)
        {
            if (string.IsNullOrWhiteSpace(pTexto)) return Result.Fail<Observacion>("Texto es requerido");
            if (string.IsNullOrWhiteSpace(pUsuario)) return Result.Fail<Observacion>("Usuario es requerido");
            Texto = pTexto;
            FechaHora = pFechaHora;
            Usuario = pUsuario;
            EnvioId = pEnvioId;
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
        /// Valida si se puede crear una instancia de Observacion
        /// </summary>
        public static Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeCrearAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede modificar esta instancia de Observacion
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeModificarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede eliminar esta instancia de Observacion
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeEliminarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }


    }
}
