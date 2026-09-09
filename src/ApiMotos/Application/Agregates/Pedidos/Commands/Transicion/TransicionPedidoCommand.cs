using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Pedidos.Commands.Transicion
{
    /// <summary>
    /// Transición del ciclo de vida del Pedido. La matriz (desde/hacia/campo) vive en
    /// ciclos-vida.json; los efectos (Kardex, Envío, comisión) en PedidoHooks.
    /// </summary>
    public record TransicionPedidoCommand(int Id, string Accion) : ICommand<Result<bool>>, IAuditableRequest
    {
        public string GetEntityType() => "Pedido";
        public string? GetEntityId() => Id.ToString();
    }
}
