using FluentResults;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiMotos.Application.Agregates.Auth.Dtos;
using ApiMotos.Domain.Agregates.Seguridad;

namespace ApiMotos.Application.Agregates.Auth.Queries.Login;

public class LoginHandler : IRequestHandler<LoginQuery, Result<LoginResponseDto>>
{
    private readonly IUsuarioRepositorio _usuarioRepositorio;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _env;

    public LoginHandler(IUsuarioRepositorio usuarioRepositorio, IConfiguration configuration, IHostEnvironment env)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _configuration = configuration;
        _env = env;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        // 0. Bypass de DESARROLLO (pablo/pablo): emite un JWT REAL, solo en Development.
        // Los seeds de usuarios nunca corren en los productos generados y el hash del
        // admin es placeholder — sin esto, [Authorize] (F4) dejaría todo en 401 en dev.
        // En Production este camino NO existe.
        if (_env.IsDevelopment() && request.Usuario == "pablo" && request.Password == "pablo")
        {
            var tokenDev = GenerarTokenDev();
            var infoDev = new UsuarioInfoDto(0, "pablo", "Desarrollador (bypass dev)",
                "pablo@dev.local", 0, "Dev", new List<string>());
            return Result.Ok(new LoginResponseDto(tokenDev, infoDev));
        }

        // 1. Buscar usuario
        var usuario = await _usuarioRepositorio.ObtenerParaLoginAsync(request.Usuario);

        if (usuario == null)
        {
            return Result.Fail("Usuario o contraseña incorrectos");
        }

        // 2. Verificar si está activo
        if (!usuario.Activo)
        {
            return Result.Fail("Usuario desactivado. Contacte al administrador");
        }

        // 3. Verificar si está bloqueado
        if (usuario.EstaBloqueado())
        {
            return Result.Fail($"Usuario bloqueado. Intente nuevamente más tarde");
        }

        // 4. Verificar contraseña
        if (!VerificarPassword(request.Password, usuario.PasswordHash))
        {
            usuario.RegistrarLoginFallido();
            await _usuarioRepositorio.ActualizarAsync(usuario);
            return Result.Fail("Usuario o contraseña incorrectos");
        }

        // 5. Login exitoso - actualizar estado
        usuario.RegistrarLoginExitoso();
        await _usuarioRepositorio.ActualizarAsync(usuario);

        // 6. Obtener capabilities
        var capabilities = usuario.ObtenerCapabilities().ToList();

        // 7. Generar token JWT
        var token = GenerarToken(usuario, capabilities);

        // 8. Construir respuesta
        var usuarioInfo = new UsuarioInfoDto(
            usuario.Id,
            usuario.UserName,
            usuario.NombreCompleto,
            usuario.Email,
            usuario.PerfilId,
            usuario.Perfil?.Nombre ?? "Sin perfil",
            capabilities
        );

        return Result.Ok(new LoginResponseDto(token, usuarioInfo));
    }

    private bool VerificarPassword(string password, string passwordHash)
    {
        // Usar BCrypt para verificar el password
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Token del bypass de desarrollo: mismas claves/issuer que los reales.</summary>
    private string GenerarTokenDev()
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "DefaultSecretKey1234567890!@#$%^&*()";
        var issuer = jwtSettings["Issuer"] ?? "ApiMotos";
        var audience = jwtSettings["Audience"] ?? "SiteMotos";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "0"),
            new(ClaimTypes.Name, "pablo"),
            new("nombre_completo", "Desarrollador (bypass dev)"),
            new("perfil_nombre", "Dev"),
            // Pase total del Banco de Reglas (autorizacion_rol): sin esto, cualquier
            // exige_rol bloquearía a los tests golden/plantilla que loguean pablo/pablo.
            new("rol", "*")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerarToken(Usuario usuario, IEnumerable<string> capabilities)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "DefaultSecretKey1234567890!@#$%^&*()";
        var issuer = jwtSettings["Issuer"] ?? "ApiMotos";
        var audience = jwtSettings["Audience"] ?? "SiteMotos";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "480");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.UserName),
            new(ClaimTypes.Email, usuario.Email),
            new("nombre_completo", usuario.NombreCompleto),
            new("perfil_id", usuario.PerfilId.ToString()),
            new("perfil_nombre", usuario.Perfil?.Nombre ?? "")
        };

        // Agregar capabilities como claims
        foreach (var capability in capabilities)
        {
            claims.Add(new Claim("capability", capability));
        }

        // Roles del perfil como claims: los consume el motor del Banco de Reglas
        // (autorizacion_rol). ObtenerParaLoginAsync ya trae Perfil→PerfilRoles→Rol.
        var roles = usuario.Perfil?.PerfilRoles
            .Where(pr => pr.Rol.Activo)
            .Select(pr => pr.Rol.Nombre)
            .Distinct(StringComparer.OrdinalIgnoreCase) ?? Enumerable.Empty<string>();
        foreach (var rol in roles)
        {
            claims.Add(new Claim("rol", rol));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
