using FluentResults;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Mutation.Queries.ObtenerHistorial;

/// <summary>
/// Query para obtener el historial de mutaciones
/// </summary>
public record ObtenerHistorialQuery(
    int? UsuarioId = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<Result<PaginatedResult<MutacionHistorialDto>>>;

/// <summary>
/// Resultado paginado genérico
/// </summary>
public record PaginatedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
