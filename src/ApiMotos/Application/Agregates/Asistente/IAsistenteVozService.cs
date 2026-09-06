using FluentResults;

namespace ApiMotos.Application.Agregates.Asistente;

/// <summary>
/// PUERTO del asistente de voz (arquitectura hexagonal): la aplicación define el contrato,
/// la infraestructura lo implementa (hoy: OpenAI Realtime). El dominio del producto no
/// sabe ni le importa qué proveedor de voz hay detrás.
/// </summary>
public interface IAsistenteVozService
{
    /// <summary>true solo si hay una API key configurada (env OPENAI_API_KEY o
    /// Asistente:OpenAIApiKey). Sin key, la capa de voz NO existe para este producto.</summary>
    bool Habilitado { get; }

    /// <summary>Crea una sesión efímera de voz: devuelve el client secret de corta vida
    /// que usa el navegador. La key maestra NUNCA sale del servidor.</summary>
    Task<Result<AsistenteSesionDto>> CrearSesionAsync(CancellationToken ct);
}

public class AsistenteSesionDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Voice { get; set; } = string.Empty;
}

public class AsistenteEstadoDto
{
    public bool Habilitado { get; set; }
}
