using FluentResults;
using MediatR;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Application.Agregates.Mutation.Queries.ObtenerHistorial;

/// <summary>
/// Handler para obtener el historial de mutaciones con paginación
/// </summary>
public class ObtenerHistorialHandler : IRequestHandler<ObtenerHistorialQuery, Result<PaginatedResult<MutacionHistorialDto>>>
{
    private readonly IMutacionRepositorio _mutacionRepositorio;

    public ObtenerHistorialHandler(IMutacionRepositorio mutacionRepositorio)
    {
        _mutacionRepositorio = mutacionRepositorio;
    }

    public async Task<Result<PaginatedResult<MutacionHistorialDto>>> Handle(
        ObtenerHistorialQuery request,
        CancellationToken cancellationToken)
    {
        var filtro = new MutacionFiltro
        {
            UsuarioId = request.UsuarioId,
            SortBy = "FechaSolicitud",
            SortDesc = true
        };

        var (items, totalCount) = await _mutacionRepositorio.ObtenerPaginadoAsync(
            filtro,
            request.Page,
            request.PageSize
        );

        var dtos = items.Select(m => new MutacionHistorialDto
        {
            Id = m.Id,
            SolicitudOriginal = m.SolicitudOriginal,
            ResumenTecnico = m.ResumenTecnico,
            Estado = m.Estado,
            FechaSolicitud = m.FechaSolicitud,
            FechaEjecucion = m.FechaEjecucion,
            UsuarioNombre = m.UsuarioNombre,
            ArchitectureCompliance = m.ArchitectureCompliance,
            CantidadArchivos = m.Archivos.Count
        }).ToList();

        return Result.Ok(new PaginatedResult<MutacionHistorialDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
