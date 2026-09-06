using FluentResults;

namespace ApiMotos.Domain.Common.ValueObjects
{
    /// <summary>
    /// Value object declarado en el spec (Ola 4a): sus invariantes viven en la
    /// factory — el estado ilegal es irrepresentable. Record CLASS (no struct):
    /// default nulo explícito, sin instancia "válida" que esquive Crear.
    /// </summary>
    public sealed record RangoAlertaSLA
    {
        public int UmbralAdvertenciaDias { get; }
        public int LimiteDias { get; }

        private RangoAlertaSLA(int umbralAdvertenciaDias, int limiteDias)
        {
            UmbralAdvertenciaDias = umbralAdvertenciaDias;
            LimiteDias = limiteDias;
        }

        public static Result<RangoAlertaSLA> Crear(int umbralAdvertenciaDias, int limiteDias)
        {
            if (limiteDias <= 0) return Result.Fail<RangoAlertaSLA>("El limite en dias debe ser mayor a cero");
            if (umbralAdvertenciaDias < 0) return Result.Fail<RangoAlertaSLA>("El umbral de advertencia no puede ser negativo");
            if (umbralAdvertenciaDias > limiteDias) return Result.Fail<RangoAlertaSLA>("El umbral de advertencia no puede superar el limite de la etapa");
            return new RangoAlertaSLA(umbralAdvertenciaDias, limiteDias);
        }
    }
}
