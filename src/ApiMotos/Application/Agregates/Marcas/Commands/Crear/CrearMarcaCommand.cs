using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Marcas.Commands.Crear
{
    public record CrearMarcaCommand(string Nombre, string Pais, bool Activo) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Marca";
        public string? GetEntityId() => null;
    }
}
