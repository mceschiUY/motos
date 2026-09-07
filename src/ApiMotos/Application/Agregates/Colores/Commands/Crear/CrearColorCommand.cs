using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Colores.Commands.Crear
{
    public record CrearColorCommand(string Nombre, string CodigoHex, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Color";
        public string? GetEntityId() => null;
    }
}
