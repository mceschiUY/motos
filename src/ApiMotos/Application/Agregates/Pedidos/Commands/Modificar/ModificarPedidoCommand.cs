using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Pedidos.Commands.Modificar
{
    public record ModificarPedidoCommand(
        int Id,
        int ClienteId,
        int VendedorId,
        int DepositoId,
        int? AgenciaId,
        DateTime? Fecha,
        string? Observaciones) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Pedido";
        public string? GetEntityId() => Id.ToString();
    }
}
