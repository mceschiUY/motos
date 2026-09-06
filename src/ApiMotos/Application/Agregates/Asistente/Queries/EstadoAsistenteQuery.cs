using MediatR;
using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Asistente.Queries;

public record EstadoAsistenteQuery() : IQuery<AsistenteEstadoDto>;

public class EstadoAsistenteHandler : IRequestHandler<EstadoAsistenteQuery, AsistenteEstadoDto>
{
    private readonly IAsistenteVozService _asistente;

    public EstadoAsistenteHandler(IAsistenteVozService asistente)
    {
        _asistente = asistente;
    }

    public Task<AsistenteEstadoDto> Handle(EstadoAsistenteQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AsistenteEstadoDto { Habilitado = _asistente.Habilitado });
    }
}
