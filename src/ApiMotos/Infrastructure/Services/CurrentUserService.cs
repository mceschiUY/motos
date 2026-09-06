// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Implementación de Usuario Actual
// Lee información del usuario desde claims JWT
// ═══════════════════════════════════════════════════════════════════════════════

using System.Security.Claims;
using ApiMotos.Shared.Abstractions;

namespace ApiMotos.Infrastructure.Services;

/// <summary>
/// Implementación de ICurrentUserService que lee claims del token JWT.
/// Cachea las capabilities por request para evitar múltiples lecturas.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private List<string>? _capabilities;

    // Claims estándar JWT
    private const string UserIdClaim = ClaimTypes.NameIdentifier;
    private const string UserNameClaim = ClaimTypes.Name;
    private const string PerfilIdClaim = "perfil_id";
    private const string CapabilityClaim = "capability";

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var claim = User?.FindFirst(UserIdClaim)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? UserName => User?.FindFirst(UserNameClaim)?.Value;

    public int? PerfilId
    {
        get
        {
            var claim = User?.FindFirst(PerfilIdClaim)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Capabilities
    {
        get
        {
            if (_capabilities == null)
            {
                _capabilities = User?
                    .FindAll(CapabilityClaim)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();
            }
            return _capabilities;
        }
    }

    public bool HasCapability(string capability)
    {
        if (string.IsNullOrWhiteSpace(capability))
            return false;

        // Comparación case-insensitive
        return Capabilities.Any(c =>
            string.Equals(c, capability, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasAnyCapability(params string[] capabilities)
    {
        if (capabilities == null || capabilities.Length == 0)
            return false;

        return capabilities.Any(HasCapability);
    }

    public bool HasAllCapabilities(params string[] capabilities)
    {
        if (capabilities == null || capabilities.Length == 0)
            return true;

        return capabilities.All(HasCapability);
    }
}

/// <summary>
/// Implementación mock para testing/desarrollo.
/// Simula un usuario con todas las capabilities.
/// </summary>
public class MockCurrentUserService : ICurrentUserService
{
    private readonly List<string> _capabilities;

    public MockCurrentUserService(IEnumerable<string>? capabilities = null)
    {
        _capabilities = capabilities?.ToList() ?? new List<string>
        {
            // Capabilities de desarrollo (acceso total)
            "Lead.Ver", "Lead.Crear", "Lead.Modificar", "Lead.Eliminar",
            "Cliente.Ver", "Cliente.Crear", "Cliente.Modificar", "Cliente.Eliminar",
            "Pedido.Ver", "Pedido.Crear", "Pedido.Modificar", "Pedido.Eliminar",
            "Usuario.Ver", "Usuario.Crear", "Usuario.Modificar", "Usuario.Eliminar",
            "Perfil.Ver", "Perfil.Crear", "Perfil.Modificar", "Perfil.Eliminar",
            "Rol.Ver", "Rol.Crear", "Rol.Modificar", "Rol.Eliminar",
            "Admin.Full"
        };
    }

    public int? UserId => 1;
    public string? UserName => "dev_user";
    public int? PerfilId => 1;
    public bool IsAuthenticated => true;
    public IReadOnlyList<string> Capabilities => _capabilities;

    public bool HasCapability(string capability) =>
        _capabilities.Any(c => string.Equals(c, capability, StringComparison.OrdinalIgnoreCase));

    public bool HasAnyCapability(params string[] capabilities) =>
        capabilities.Any(HasCapability);

    public bool HasAllCapabilities(params string[] capabilities) =>
        capabilities.All(HasCapability);
}
