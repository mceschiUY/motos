// ═══════════════════════════════════════════════════════════════════════════════
// SISTEMA DE SEGURIDAD ZAS - Entidad Usuario
// Usuario del sistema con un Perfil asignado
// ═══════════════════════════════════════════════════════════════════════════════

using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Seguridad;

/// <summary>
/// Usuario: Cuenta de acceso al sistema con perfil asignado.
/// Cada usuario tiene un único perfil que determina sus permisos.
/// </summary>
public class Usuario : BaseEntity<int>
{
    /// <summary>
    /// Nombre de usuario único para login
    /// </summary>
    public string UserName { get; private set; } = string.Empty;

    /// <summary>
    /// Email del usuario
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto { get; private set; } = string.Empty;

    /// <summary>
    /// Hash de la contraseña (BCrypt)
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// ID del perfil asignado
    /// </summary>
    public int PerfilId { get; private set; }

    /// <summary>
    /// Perfil del usuario (navegación)
    /// </summary>
    public virtual Perfil Perfil { get; private set; } = null!;

    /// <summary>
    /// Indica si el usuario está activo
    /// </summary>
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// Fecha de creación de la cuenta
    /// </summary>
    public DateTime FechaCreacion { get; private set; } = DateTime.Now;

    /// <summary>
    /// Fecha del último login
    /// </summary>
    public DateTime? UltimoLogin { get; private set; }

    /// <summary>
    /// Número de intentos fallidos de login (para bloqueo)
    /// </summary>
    public int IntentosFallidos { get; private set; } = 0;

    /// <summary>
    /// Fecha de bloqueo temporal (null si no está bloqueado)
    /// </summary>
    public DateTime? BloqueadoHasta { get; private set; }

    // Constructor para EF
    protected Usuario() { }

    public Usuario(string userName, string email, string nombreCompleto, int perfilId)
    {
        SetUserName(userName);
        SetEmail(email);
        NombreCompleto = nombreCompleto;
        PerfilId = perfilId;
    }

    public void SetUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(userName));

        UserName = userName.Trim().ToLowerInvariant();
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email no puede estar vacío", nameof(email));

        // Validación básica de email
        if (!email.Contains('@'))
            throw new ArgumentException("Email inválido", nameof(email));

        Email = email.Trim().ToLowerInvariant();
    }

    public void SetPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de contraseña no puede estar vacío", nameof(passwordHash));

        PasswordHash = passwordHash;
    }

    public void CambiarPerfil(int perfilId)
    {
        if (perfilId <= 0)
            throw new ArgumentException("ID de perfil inválido", nameof(perfilId));

        PerfilId = perfilId;
    }

    public void CambiarPerfil(Perfil perfil)
    {
        Perfil = perfil ?? throw new ArgumentNullException(nameof(perfil));
        PerfilId = perfil.Id;
    }

    public void ActualizarNombreCompleto(string nombreCompleto)
    {
        NombreCompleto = nombreCompleto ?? string.Empty;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    /// <summary>
    /// Registra un login exitoso
    /// </summary>
    public void RegistrarLoginExitoso()
    {
        UltimoLogin = DateTime.Now;
        IntentosFallidos = 0;
        BloqueadoHasta = null;
    }

    /// <summary>
    /// Registra un intento fallido de login
    /// </summary>
    /// <param name="maxIntentos">Máximo de intentos antes de bloquear</param>
    /// <param name="minutosBloqueio">Minutos de bloqueo</param>
    public void RegistrarLoginFallido(int maxIntentos = 5, int minutosBloqueio = 15)
    {
        IntentosFallidos++;

        if (IntentosFallidos >= maxIntentos)
        {
            BloqueadoHasta = DateTime.Now.AddMinutes(minutosBloqueio);
        }
    }

    /// <summary>
    /// Verifica si el usuario está bloqueado
    /// </summary>
    public bool EstaBloqueado()
    {
        if (!BloqueadoHasta.HasValue)
            return false;

        if (DateTime.Now > BloqueadoHasta.Value)
        {
            // El bloqueo expiró
            BloqueadoHasta = null;
            IntentosFallidos = 0;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Desbloquea al usuario manualmente
    /// </summary>
    public void Desbloquear()
    {
        BloqueadoHasta = null;
        IntentosFallidos = 0;
    }

    /// <summary>
    /// Obtiene todas las capabilities del usuario a través de su perfil
    /// </summary>
    public IEnumerable<string> ObtenerCapabilities()
    {
        return Perfil?.ObtenerCapabilities() ?? Enumerable.Empty<string>();
    }

    /// <summary>
    /// Verifica si el usuario tiene una capability específica
    /// </summary>
    public bool TieneCapability(string capability)
    {
        return Perfil?.TieneCapability(capability) ?? false;
    }
}
