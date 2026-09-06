using Microsoft.AspNetCore.Mvc;
using ApiMotos.Domain.Agregates.Seguridad;
using ApiMotos.Infrastructure.Agregates.Seguridad.Persistence;

using Microsoft.AspNetCore.Authorization;
namespace ApiMotos.Controllers;

/// <summary>
/// Controller para administracion del sistema de seguridad (Usuarios, Perfiles, Roles, Capabilities)
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SeguridadController : ControllerBase
{
    private readonly ICapabilityRepositorio _capabilityRepo;
    private readonly IRolRepositorio _rolRepo;
    private readonly IPerfilRepositorio _perfilRepo;
    private readonly IUsuarioRepositorio _usuarioRepo;
    private readonly ILogger<SeguridadController> _logger;

    public SeguridadController(
        ICapabilityRepositorio capabilityRepo,
        IRolRepositorio rolRepo,
        IPerfilRepositorio perfilRepo,
        IUsuarioRepositorio usuarioRepo,
        ILogger<SeguridadController> logger)
    {
        _capabilityRepo = capabilityRepo;
        _rolRepo = rolRepo;
        _perfilRepo = perfilRepo;
        _usuarioRepo = usuarioRepo;
        _logger = logger;
    }

    // =============================================================================
    // CAPABILITIES
    // =============================================================================

    /// <summary>
    /// Obtiene todas las capabilities
    /// </summary>
    [HttpGet("capabilities")]
    public async Task<ActionResult<IEnumerable<CapabilityDto>>> GetCapabilities()
    {
        var capabilities = await _capabilityRepo.ObtenerTodosAsync();
        return Ok(capabilities.Select(c => new CapabilityDto(c)));
    }

    /// <summary>
    /// Obtiene capabilities por modulo
    /// </summary>
    [HttpGet("capabilities/modulo/{modulo}")]
    public async Task<ActionResult<IEnumerable<CapabilityDto>>> GetCapabilitiesByModulo(string modulo)
    {
        var capabilities = await _capabilityRepo.ObtenerPorModuloAsync(modulo);
        return Ok(capabilities.Select(c => new CapabilityDto(c)));
    }

    /// <summary>
    /// Obtiene lista de modulos disponibles
    /// </summary>
    [HttpGet("capabilities/modulos")]
    public async Task<ActionResult<IEnumerable<string>>> GetModulos()
    {
        var modulos = await _capabilityRepo.ObtenerModulosAsync();
        return Ok(modulos);
    }

    // =============================================================================
    // ROLES
    // =============================================================================

    /// <summary>
    /// Obtiene todos los roles
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<RolDto>>> GetRoles()
    {
        var roles = await _rolRepo.ObtenerTodosAsync();
        return Ok(roles.Select(r => new RolDto(r)));
    }

    /// <summary>
    /// Obtiene un rol por ID con sus capabilities
    /// </summary>
    [HttpGet("roles/{id}")]
    public async Task<ActionResult<RolDto>> GetRol(int id)
    {
        var rol = await _rolRepo.ObtenerPorIdConCapabilitiesAsync(id);
        if (rol == null)
            return NotFound();

        return Ok(new RolDto(rol));
    }

    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    [HttpPost("roles")]
    public async Task<ActionResult<int>> CreateRol([FromBody] CrearRolRequest request)
    {
        var existente = await _rolRepo.ObtenerPorNombreAsync(request.Nombre);
        if (existente != null)
            return BadRequest("Ya existe un rol con ese nombre");

        var rol = new Rol(request.Nombre, request.Descripcion ?? "");
        var id = await _rolRepo.CrearAsync(rol);

        _logger.LogInformation("Rol creado: {Nombre} (ID: {Id})", request.Nombre, id);
        return Ok(id);
    }

    /// <summary>
    /// Modifica un rol
    /// </summary>
    [HttpPut("roles/{id}")]
    public async Task<ActionResult> UpdateRol(int id, [FromBody] CrearRolRequest request)
    {
        var rol = await _rolRepo.ObtenerPorIdAsync(id);
        if (rol == null)
            return NotFound();

        var existente = await _rolRepo.ObtenerPorNombreAsync(request.Nombre);
        if (existente != null && existente.Id != id)
            return BadRequest("Ya existe otro rol con ese nombre");

        rol.SetNombre(request.Nombre);
        rol.ActualizarDescripcion(request.Descripcion ?? "");
        await _rolRepo.ActualizarAsync(rol);

        return Ok();
    }

    /// <summary>
    /// Elimina un rol
    /// </summary>
    [HttpDelete("roles/{id}")]
    public async Task<ActionResult> DeleteRol(int id)
    {
        var rol = await _rolRepo.ObtenerPorIdAsync(id);
        if (rol == null)
            return NotFound();

        // Desactivar en lugar de eliminar
        rol.Desactivar();
        await _rolRepo.ActualizarAsync(rol);

        _logger.LogInformation("Rol desactivado: {Nombre} (ID: {Id})", rol.Nombre, id);
        return Ok();
    }

    /// <summary>
    /// Toggle activo/inactivo de un rol
    /// </summary>
    [HttpPut("roles/{id}/toggle-activo")]
    public async Task<ActionResult> ToggleRolActivo(int id)
    {
        var rol = await _rolRepo.ObtenerPorIdAsync(id);
        if (rol == null)
            return NotFound();

        if (rol.Activo)
            rol.Desactivar();
        else
            rol.Activar();

        await _rolRepo.ActualizarAsync(rol);
        return Ok();
    }

    /// <summary>
    /// Obtiene capabilities de un rol
    /// </summary>
    [HttpGet("roles/{id}/capabilities")]
    public async Task<ActionResult<IEnumerable<CapabilityDto>>> GetRolCapabilities(int id)
    {
        var rol = await _rolRepo.ObtenerPorIdConCapabilitiesAsync(id);
        if (rol == null)
            return NotFound();

        var capabilities = rol.RolCapabilities.Select(rc => new CapabilityDto(rc.Capability));
        return Ok(capabilities);
    }

    /// <summary>
    /// Asigna capabilities a un rol (reemplaza las existentes)
    /// </summary>
    [HttpPut("roles/{id}/capabilities")]
    public async Task<ActionResult> AsignarCapabilities(int id, [FromBody] AsignarCapabilitiesRequest request)
    {
        var rol = await _rolRepo.ObtenerPorIdAsync(id);
        if (rol == null)
            return NotFound();

        var repoConReemplazo = _rolRepo as RolRepositorio;
        if (repoConReemplazo != null)
        {
            await repoConReemplazo.ReemplazarCapabilitiesAsync(id, request.CapabilityIds);
        }

        _logger.LogInformation("Capabilities actualizadas para rol {RolId}: {Count} capabilities", id, request.CapabilityIds.Count());
        return Ok();
    }

    // =============================================================================
    // PERFILES
    // =============================================================================

    /// <summary>
    /// Obtiene todos los perfiles
    /// </summary>
    [HttpGet("perfiles")]
    public async Task<ActionResult<IEnumerable<PerfilDto>>> GetPerfiles()
    {
        var perfiles = await _perfilRepo.ObtenerTodosAsync();
        return Ok(perfiles.Select(p => new PerfilDto(p)));
    }

    /// <summary>
    /// Obtiene un perfil por ID con sus roles
    /// </summary>
    [HttpGet("perfiles/{id}")]
    public async Task<ActionResult<PerfilDto>> GetPerfil(int id)
    {
        var perfil = await _perfilRepo.ObtenerPorIdConRolesAsync(id);
        if (perfil == null)
            return NotFound();

        return Ok(new PerfilDto(perfil));
    }

    /// <summary>
    /// Crea un nuevo perfil
    /// </summary>
    [HttpPost("perfiles")]
    public async Task<ActionResult<int>> CreatePerfil([FromBody] CrearPerfilRequest request)
    {
        var existente = await _perfilRepo.ObtenerPorNombreAsync(request.Nombre);
        if (existente != null)
            return BadRequest("Ya existe un perfil con ese nombre");

        var perfil = new Perfil(request.Nombre, request.Descripcion ?? "");
        var id = await _perfilRepo.CrearAsync(perfil);

        _logger.LogInformation("Perfil creado: {Nombre} (ID: {Id})", request.Nombre, id);
        return Ok(id);
    }

    /// <summary>
    /// Modifica un perfil
    /// </summary>
    [HttpPut("perfiles/{id}")]
    public async Task<ActionResult> UpdatePerfil(int id, [FromBody] CrearPerfilRequest request)
    {
        var perfil = await _perfilRepo.ObtenerPorIdAsync(id);
        if (perfil == null)
            return NotFound();

        var existente = await _perfilRepo.ObtenerPorNombreAsync(request.Nombre);
        if (existente != null && existente.Id != id)
            return BadRequest("Ya existe otro perfil con ese nombre");

        perfil.SetNombre(request.Nombre);
        perfil.ActualizarDescripcion(request.Descripcion ?? "");
        await _perfilRepo.ActualizarAsync(perfil);

        return Ok();
    }

    /// <summary>
    /// Elimina un perfil
    /// </summary>
    [HttpDelete("perfiles/{id}")]
    public async Task<ActionResult> DeletePerfil(int id)
    {
        var perfil = await _perfilRepo.ObtenerPorIdAsync(id);
        if (perfil == null)
            return NotFound();

        var repoConConteo = _perfilRepo as PerfilRepositorio;
        if (repoConConteo != null)
        {
            var cantidadUsuarios = await repoConConteo.ContarUsuariosPorPerfilAsync(id);
            if (cantidadUsuarios > 0)
                return BadRequest($"No se puede eliminar el perfil porque tiene {cantidadUsuarios} usuario(s) asignado(s)");
        }

        // Desactivar en lugar de eliminar
        perfil.Desactivar();
        await _perfilRepo.ActualizarAsync(perfil);

        _logger.LogInformation("Perfil desactivado: {Nombre} (ID: {Id})", perfil.Nombre, id);
        return Ok();
    }

    /// <summary>
    /// Toggle activo/inactivo de un perfil
    /// </summary>
    [HttpPut("perfiles/{id}/toggle-activo")]
    public async Task<ActionResult> TogglePerfilActivo(int id)
    {
        var perfil = await _perfilRepo.ObtenerPorIdAsync(id);
        if (perfil == null)
            return NotFound();

        if (perfil.Activo)
            perfil.Desactivar();
        else
            perfil.Activar();

        await _perfilRepo.ActualizarAsync(perfil);
        return Ok();
    }

    /// <summary>
    /// Obtiene roles de un perfil
    /// </summary>
    [HttpGet("perfiles/{id}/roles")]
    public async Task<ActionResult<IEnumerable<RolDto>>> GetPerfilRoles(int id)
    {
        var perfil = await _perfilRepo.ObtenerPorIdConRolesAsync(id);
        if (perfil == null)
            return NotFound();

        var roles = perfil.PerfilRoles.Select(pr => new RolDto(pr.Rol));
        return Ok(roles);
    }

    /// <summary>
    /// Asigna roles a un perfil (reemplaza los existentes)
    /// </summary>
    [HttpPut("perfiles/{id}/roles")]
    public async Task<ActionResult> AsignarRoles(int id, [FromBody] AsignarRolesRequest request)
    {
        var perfil = await _perfilRepo.ObtenerPorIdAsync(id);
        if (perfil == null)
            return NotFound();

        var repoConReemplazo = _perfilRepo as PerfilRepositorio;
        if (repoConReemplazo != null)
        {
            await repoConReemplazo.ReemplazarRolesAsync(id, request.RolIds);
        }

        _logger.LogInformation("Roles actualizados para perfil {PerfilId}: {Count} roles", id, request.RolIds.Count());
        return Ok();
    }

    // =============================================================================
    // USUARIOS
    // =============================================================================

    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    [HttpGet("usuarios")]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
    {
        var usuarios = await _usuarioRepo.ObtenerTodosAsync();
        return Ok(usuarios.Select(u => new UsuarioDto(u)));
    }

    /// <summary>
    /// Obtiene un usuario por ID
    /// </summary>
    [HttpGet("usuarios/{id}")]
    public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdConPerfilAsync(id);
        if (usuario == null)
            return NotFound();

        return Ok(new UsuarioDto(usuario));
    }

    /// <summary>
    /// Crea un nuevo usuario
    /// </summary>
    [HttpPost("usuarios")]
    public async Task<ActionResult<int>> CreateUsuario([FromBody] CrearUsuarioRequest request)
    {
        if (await _usuarioRepo.ExisteUserNameAsync(request.UserName))
            return BadRequest("Ya existe un usuario con ese nombre de usuario");

        if (await _usuarioRepo.ExisteEmailAsync(request.Email))
            return BadRequest("Ya existe un usuario con ese email");

        var perfil = await _perfilRepo.ObtenerPorIdAsync(request.PerfilId);
        if (perfil == null)
            return BadRequest("El perfil especificado no existe");

        var usuario = new Usuario(request.UserName, request.Email, request.NombreCompleto, request.PerfilId);
        usuario.SetPassword(BCrypt.Net.BCrypt.HashPassword(request.Password));

        var id = await _usuarioRepo.CrearAsync(usuario);

        _logger.LogInformation("Usuario creado: {UserName} (ID: {Id})", request.UserName, id);
        return Ok(id);
    }

    /// <summary>
    /// Modifica un usuario
    /// </summary>
    [HttpPut("usuarios/{id}")]
    public async Task<ActionResult> UpdateUsuario(int id, [FromBody] ModificarUsuarioRequest request)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
        if (usuario == null)
            return NotFound();

        // Verificar username unico
        var existenteUserName = await _usuarioRepo.ObtenerPorUserNameAsync(request.UserName);
        if (existenteUserName != null && existenteUserName.Id != id)
            return BadRequest("Ya existe otro usuario con ese nombre de usuario");

        // Verificar email unico
        var existenteEmail = await _usuarioRepo.ObtenerPorEmailAsync(request.Email);
        if (existenteEmail != null && existenteEmail.Id != id)
            return BadRequest("Ya existe otro usuario con ese email");

        var perfil = await _perfilRepo.ObtenerPorIdAsync(request.PerfilId);
        if (perfil == null)
            return BadRequest("El perfil especificado no existe");

        usuario.SetUserName(request.UserName);
        usuario.SetEmail(request.Email);
        usuario.ActualizarNombreCompleto(request.NombreCompleto);
        usuario.CambiarPerfil(request.PerfilId);

        await _usuarioRepo.ActualizarAsync(usuario);

        return Ok();
    }

    /// <summary>
    /// Elimina (desactiva) un usuario
    /// </summary>
    [HttpDelete("usuarios/{id}")]
    public async Task<ActionResult> DeleteUsuario(int id)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
        if (usuario == null)
            return NotFound();

        usuario.Desactivar();
        await _usuarioRepo.ActualizarAsync(usuario);

        _logger.LogInformation("Usuario desactivado: {UserName} (ID: {Id})", usuario.UserName, id);
        return Ok();
    }

    /// <summary>
    /// Toggle activo/inactivo de un usuario
    /// </summary>
    [HttpPut("usuarios/{id}/toggle-activo")]
    public async Task<ActionResult> ToggleUsuarioActivo(int id)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
        if (usuario == null)
            return NotFound();

        if (usuario.Activo)
            usuario.Desactivar();
        else
            usuario.Activar();

        await _usuarioRepo.ActualizarAsync(usuario);
        return Ok();
    }

    /// <summary>
    /// Cambia la contraseña de un usuario
    /// </summary>
    [HttpPut("usuarios/{id}/password")]
    public async Task<ActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordRequest request)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
        if (usuario == null)
            return NotFound();

        usuario.SetPassword(BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword));
        await _usuarioRepo.ActualizarAsync(usuario);

        _logger.LogInformation("Password cambiado para usuario: {UserName} (ID: {Id})", usuario.UserName, id);
        return Ok();
    }

    /// <summary>
    /// Desbloquea un usuario bloqueado
    /// </summary>
    [HttpPut("usuarios/{id}/desbloquear")]
    public async Task<ActionResult> DesbloquearUsuario(int id)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(id);
        if (usuario == null)
            return NotFound();

        usuario.Desbloquear();
        await _usuarioRepo.ActualizarAsync(usuario);

        _logger.LogInformation("Usuario desbloqueado: {UserName} (ID: {Id})", usuario.UserName, id);
        return Ok();
    }
}

// =============================================================================
// DTOs
// =============================================================================

public record CapabilityDto(int Id, string Nombre, string Descripcion, string Modulo, bool Activo)
{
    public CapabilityDto(Capability c) : this(c.Id, c.Nombre, c.Descripcion, c.Modulo, c.Activo) { }
}

public record RolDto(int Id, string Nombre, string Descripcion, bool Activo, DateTime FechaCreacion, int CantidadCapabilities, IEnumerable<CapabilityDto>? Capabilities = null)
{
    public RolDto(Rol r) : this(
        r.Id,
        r.Nombre,
        r.Descripcion,
        r.Activo,
        r.FechaCreacion,
        r.RolCapabilities?.Count ?? 0,
        r.RolCapabilities?.Select(rc => new CapabilityDto(rc.Capability))) { }
}

public record PerfilDto(int Id, string Nombre, string Descripcion, bool Activo, DateTime FechaCreacion, int CantidadRoles, int CantidadUsuarios, IEnumerable<RolDto>? Roles = null)
{
    public PerfilDto(Perfil p) : this(
        p.Id,
        p.Nombre,
        p.Descripcion,
        p.Activo,
        p.FechaCreacion,
        p.PerfilRoles?.Count ?? 0,
        p.Usuarios?.Count ?? 0,
        p.PerfilRoles?.Select(pr => new RolDto(pr.Rol))) { }
}

public record UsuarioDto(int Id, string UserName, string Email, string NombreCompleto, int PerfilId, string? PerfilNombre, bool Activo, DateTime FechaCreacion, DateTime? UltimoLogin, DateTime? BloqueadoHasta)
{
    public UsuarioDto(Usuario u) : this(
        u.Id,
        u.UserName,
        u.Email,
        u.NombreCompleto,
        u.PerfilId,
        u.Perfil?.Nombre,
        u.Activo,
        u.FechaCreacion,
        u.UltimoLogin,
        u.BloqueadoHasta) { }
}

// =============================================================================
// Request DTOs
// =============================================================================

public record CrearRolRequest(string Nombre, string? Descripcion);
public record CrearPerfilRequest(string Nombre, string? Descripcion);
public record AsignarCapabilitiesRequest(IEnumerable<int> CapabilityIds);
public record AsignarRolesRequest(IEnumerable<int> RolIds);
public record CrearUsuarioRequest(string UserName, string Email, string NombreCompleto, string Password, int PerfilId);
public record ModificarUsuarioRequest(string UserName, string Email, string NombreCompleto, int PerfilId);
public record CambiarPasswordRequest(string NuevaPassword);
