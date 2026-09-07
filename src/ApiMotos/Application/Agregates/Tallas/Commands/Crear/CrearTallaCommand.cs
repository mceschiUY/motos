using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Tallas.Commands.Crear
{
    public record CrearTallaCommand(string Nombre, string Tipo, int Orden, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Talla";
        public string? GetEntityId() => null;
    }
}
