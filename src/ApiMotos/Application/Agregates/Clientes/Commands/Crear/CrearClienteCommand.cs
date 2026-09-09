using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Application.Agregates.Clientes.Commands.Crear
{
    // Etapa A: los campos comerciales son opcionales (default null) para que los payloads
    // existentes {nombre, telefono, direccionEntrega} sigan funcionando sin cambios.
    public record CrearClienteCommand(
        string Nombre,
        string Telefono,
        string DireccionEntrega,
        string? Tipo = null,
        string? Ciudad = null,
        string? Contacto = null,
        string? Email = null,
        int? VendedorId = null,
        string? Notas = null,
        decimal? Latitud = null,
        decimal? Longitud = null) : ICommand<Result<int>>, IAuditableRequest
    {
        public string GetEntityType() => "Cliente";
        public string? GetEntityId() => null;
    }
}
