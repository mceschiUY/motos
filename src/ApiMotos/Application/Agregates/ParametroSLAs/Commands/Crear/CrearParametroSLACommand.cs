using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.ParametroSLAs.Commands.Crear
{
    public record CrearParametroSLACommand(int RangoAlertaUmbralAdvertenciaDias, int RangoAlertaLimiteDias) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "ParametroSLA";
        public string? GetEntityId() => null;
    }
}
