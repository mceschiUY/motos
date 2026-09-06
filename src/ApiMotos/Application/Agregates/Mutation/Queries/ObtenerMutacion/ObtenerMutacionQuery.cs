using FluentResults;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Mutation.Queries.ObtenerMutacion;

/// <summary>
/// Query para obtener una mutación por ID con todos sus detalles
/// </summary>
public record ObtenerMutacionQuery(int MutacionId) : IQuery<Result<MutacionResponseDto>>;
