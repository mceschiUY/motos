using System.Text;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Configuration;
using ApiMotos.Application.Agregates.Asistente;

namespace ApiMotos.Infrastructure.Services;

/// <summary>
/// ADAPTADOR OpenAI Realtime del puerto IAsistenteVozService (hexagonal: este es el único
/// archivo del sistema que sabe que existe OpenAI). Crea client secrets EFÍMEROS via
/// POST /v1/realtime/client_secrets — la key maestra vive en el server (env
/// OPENAI_API_KEY o Asistente:OpenAIApiKey) y jamás viaja al navegador.
/// </summary>
public class OpenAIRealtimeService : IAsistenteVozService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string? _apiKey;
    private readonly string _model;
    private readonly string _voice;

    public OpenAIRealtimeService(IHttpClientFactory httpFactory, IConfiguration configuration)
    {
        _httpFactory = httpFactory;
        _apiKey = configuration["Asistente:OpenAIApiKey"]
                  ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        _model = configuration["Asistente:Model"] ?? "gpt-realtime-2.1-mini";
        _voice = configuration["Asistente:Voice"] ?? "marin";
    }

    public bool Habilitado => !string.IsNullOrWhiteSpace(_apiKey);

    public async Task<Result<AsistenteSesionDto>> CrearSesionAsync(CancellationToken ct)
    {
        if (!Habilitado)
            return Result.Fail("El asistente de voz no está configurado en este producto");

        var body = new
        {
            session = new
            {
                type = "realtime",
                model = _model,
                audio = new { output = new { voice = _voice } }
            }
        };

        var http = _httpFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/realtime/client_secrets");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        HttpResponseMessage resp;
        try
        {
            resp = await http.SendAsync(req, ct);
        }
        catch (Exception ex)
        {
            return Result.Fail($"No se pudo contactar al proveedor de voz: {ex.Message}");
        }

        var contenido = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            // Mensaje honesto y accionable (cuota agotada, key inválida, etc.)
            var detalle = ExtraerMensajeError(contenido) ?? resp.StatusCode.ToString();
            return Result.Fail($"El proveedor de voz rechazó la sesión: {detalle}");
        }

        var secret = ExtraerClientSecret(contenido);
        if (secret is null)
            return Result.Fail("Respuesta inesperada del proveedor de voz (sin client secret)");

        return Result.Ok(new AsistenteSesionDto
        {
            ClientSecret = secret,
            Model = _model,
            Voice = _voice
        });
    }

    /// <summary>El secret llega como "value" en la raíz (formato vigente) o anidado en
    /// "client_secret.value" (formatos previos) — se toleran ambos.</summary>
    private static string? ExtraerClientSecret(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("value", out var v) && v.ValueKind == JsonValueKind.String)
                return v.GetString();
            if (doc.RootElement.TryGetProperty("client_secret", out var cs))
            {
                if (cs.ValueKind == JsonValueKind.String) return cs.GetString();
                if (cs.TryGetProperty("value", out var v2)) return v2.GetString();
            }
        }
        catch { /* respuesta no-JSON */ }
        return null;
    }

    private static string? ExtraerMensajeError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var err) &&
                err.TryGetProperty("message", out var msg))
                return msg.GetString();
        }
        catch { /* respuesta no-JSON */ }
        return null;
    }
}
