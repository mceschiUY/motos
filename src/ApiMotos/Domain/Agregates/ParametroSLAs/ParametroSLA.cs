using FluentResults;
using ApiMotos.Domain.Common;
using ApiMotos.Domain.Common.ValueObjects;

namespace ApiMotos.Domain.Agregates.ParametroSLAs
{
    public class ParametroSLA : BaseEntity<int>
    {
        private ParametroSLA() : base()
        {
            Etapa = string.Empty;
            RangoAlertaUmbralAdvertenciaDias = 0;
            RangoAlertaLimiteDias = 0;
        }

        private ParametroSLA(string pEtapa, int pRangoAlertaUmbralAdvertenciaDias, int pRangoAlertaLimiteDias)
        {
            Etapa = pEtapa;
            RangoAlertaUmbralAdvertenciaDias = pRangoAlertaUmbralAdvertenciaDias;
            RangoAlertaLimiteDias = pRangoAlertaLimiteDias;
        }

        public string Etapa { get; private set; }
        public int RangoAlertaUmbralAdvertenciaDias { get; private set; }
        public int RangoAlertaLimiteDias { get; private set; }

        public static Result<ParametroSLA> Crear(string etapa, int rangoAlertaUmbralAdvertenciaDias, int rangoAlertaLimiteDias)
        {
            if (string.IsNullOrWhiteSpace(etapa)) return Result.Fail<ParametroSLA>("Etapa es requerido");
            var vRangoAlerta = RangoAlertaSLA.Crear(rangoAlertaUmbralAdvertenciaDias, rangoAlertaLimiteDias);
            if (vRangoAlerta.IsFailed) return Result.Fail<ParametroSLA>(vRangoAlerta.Errors[0].Message);
            return new ParametroSLA(etapa, rangoAlertaUmbralAdvertenciaDias, rangoAlertaLimiteDias);
        }

        public Result<ParametroSLA> Modificar(string pEtapa, int pRangoAlertaUmbralAdvertenciaDias, int pRangoAlertaLimiteDias)
        {
            if (string.IsNullOrWhiteSpace(pEtapa)) return Result.Fail<ParametroSLA>("Etapa es requerido");
            var vRangoAlerta = RangoAlertaSLA.Crear(pRangoAlertaUmbralAdvertenciaDias, pRangoAlertaLimiteDias);
            if (vRangoAlerta.IsFailed) return Result.Fail<ParametroSLA>(vRangoAlerta.Errors[0].Message);
            Etapa = pEtapa;
            RangoAlertaUmbralAdvertenciaDias = pRangoAlertaUmbralAdvertenciaDias;
            RangoAlertaLimiteDias = pRangoAlertaLimiteDias;
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
        /// Valida si se puede crear una instancia de ParametroSLA
        /// </summary>
        public static Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeCrearAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede modificar esta instancia de ParametroSLA
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeModificarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }

        /// <summary>
        /// Valida si se puede eliminar esta instancia de ParametroSLA
        /// </summary>
        public Task<ApiMotos.Application.Common.Validation.BusinessValidationResult> PuedeEliminarAsync(CancellationToken ct = default)
        {
            // Sin validadores definidos - siempre permitido
            return Task.FromResult(ApiMotos.Application.Common.Validation.BusinessValidationResult.Success());
        }


    }
}
