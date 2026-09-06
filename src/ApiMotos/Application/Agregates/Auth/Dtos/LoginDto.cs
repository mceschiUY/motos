namespace ApiMotos.Application.Agregates.Auth.Dtos;

public record LoginRequestDto(
    string Usuario,
    string Password
);

public record LoginResponseDto(
    string Token,
    UsuarioInfoDto Usuario
);

public record UsuarioInfoDto(
    int Id,
    string NombreUsuario,
    string NombreCompleto,
    string Email,
    int PerfilId,
    string PerfilNombre,
    IEnumerable<string> Capabilities
);
