using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Actividades.Commands.Crear
{
    public record CrearActividadCommand(
        int VendedorId,
        int ClienteId,
        string Tipo,
        DateTime? Fecha,
        string Resultado,
        string? Notas,
        DateTime? ProximaAccion,
        int? PedidoId) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Actividad";
        public string? GetEntityId() => null;
    }
}
