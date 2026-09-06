using FluentResults;
using MediatR;
using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Asistente.Commands;

/// <summary>Abre una sesión efímera de voz (consume saldo del proveedor → es un Command).</summary>
public record CrearSesionAsistenteCommand() : ICommand<Result<AsistenteSesionDto>>;

public class CrearSesionAsistenteHandler : IRequestHandler<CrearSesionAsistenteCommand, Result<AsistenteSesionDto>>
{
    private readonly IAsistenteVozService _asistente;

    public CrearSesionAsistenteHandler(IAsistenteVozService asistente)
    {
        _asistente = asistente;
    }

    public Task<Result<AsistenteSesionDto>> Handle(CrearSesionAsistenteCommand request, CancellationToken cancellationToken)
    {
        return _asistente.CrearSesionAsync(cancellationToken);
    }
}
