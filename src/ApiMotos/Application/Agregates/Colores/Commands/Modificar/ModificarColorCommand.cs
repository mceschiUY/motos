using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Colores.Commands.Modificar
{
    public record ModificarColorCommand(int Id, string Nombre, string CodigoHex, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Color";
        public string? GetEntityId() => Id.ToString();
    }
}
