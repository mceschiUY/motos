using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Envios.Commands.Crear
{
    public record CrearEnvioCommand(string CodigoRastreo, DateTime FechaRecibido, DateTime FechaFactura, DateTime FechaEnvio, DateTime FechaEntrega, string MotivoAnulacion, int ClienteId, int AgenciaId) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Envio";
        public string? GetEntityId() => null;
    }
}
