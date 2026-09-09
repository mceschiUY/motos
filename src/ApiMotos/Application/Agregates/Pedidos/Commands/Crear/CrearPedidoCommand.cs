using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Pedidos.Commands.Crear
{
    /// <summary>Alta de pedido: solo cabecera. Numero, Estado, TotalUsd y ComisionUsd
    /// son del sistema (PedidoHooks y las líneas), no vienen del form.</summary>
    public record CrearPedidoCommand(
        int ClienteId,
        int VendedorId,
        int DepositoId,
        int? AgenciaId,
        DateTime? Fecha,
        string? Observaciones) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Pedido";
        public string? GetEntityId() => null;
    }
}
