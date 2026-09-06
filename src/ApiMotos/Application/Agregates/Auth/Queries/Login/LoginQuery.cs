using FluentResults;
using ApiMotos.Application.Common;
using ApiMotos.Application.Agregates.Auth.Dtos;
using ApiMotos.Shared.Infrastructure;

namespace ApiMotos.Application.Agregates.Auth.Queries.Login;

[ZasAllowAnonymous]
public record LoginQuery(string Usuario, string Password) : IQuery<Result<LoginResponseDto>>;
