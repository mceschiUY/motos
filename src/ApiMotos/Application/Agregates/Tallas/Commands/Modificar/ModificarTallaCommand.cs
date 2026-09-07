using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Tallas.Commands.Modificar
{
    public record ModificarTallaCommand(int Id, string Nombre, string Tipo, int Orden, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Talla";
        public string? GetEntityId() => Id.ToString();
    }
}
