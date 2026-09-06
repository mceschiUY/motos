using Microsoft.AspNetCore.Mvc;
using ApiMotos.Domain.Agregates.Configuracion;

using Microsoft.AspNetCore.Authorization;
namespace ApiMotos.Controllers;

/// <summary>
/// Controller para administracion de la configuracion del sitio
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionRepositorio _repo;
    private readonly ILogger<ConfiguracionController> _logger;

    public ConfiguracionController(
        IConfiguracionRepositorio repo,
        ILogger<ConfiguracionController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // =============================================================================
    // LECTURA
    // =============================================================================

    /// <summary>
    /// Obtiene todas las configuraciones activas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfiguracionDto>>> GetAll()
    {
        var configs = await _repo.ObtenerTodosAsync();
        return Ok(configs.Select(c => new ConfiguracionDto(c)));
    }

    /// <summary>
    /// Obtiene todas las configuraciones como diccionario (para frontend)
    /// Endpoint publico - no requiere autenticacion
    /// </summary>
    [HttpGet("publica")]
    public async Task<ActionResult<Dictionary<string, string>>> GetPublica()
    {
        var configs = await _repo.ObtenerTodosComoDiccionarioAsync();
        return Ok(configs);
    }

    /// <summary>
    /// Obtiene configuraciones por grupo
    /// </summary>
    [HttpGet("grupo/{grupo}")]
    public async Task<ActionResult<IEnumerable<ConfiguracionDto>>> GetByGrupo(string grupo)
    {
        var configs = await _repo.ObtenerPorGrupoAsync(grupo);
        return Ok(configs.Select(c => new ConfiguracionDto(c)));
    }

    /// <summary>
    /// Obtiene una configuracion por ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConfiguracionDto>> GetById(int id)
    {
        var config = await _repo.ObtenerPorIdAsync(id);
        if (config == null)
            return NotFound();

        return Ok(new ConfiguracionDto(config));
    }

    /// <summary>
    /// Obtiene una configuracion por clave
    /// </summary>
    [HttpGet("clave/{clave}")]
    public async Task<ActionResult<ConfiguracionDto>> GetByClave(string clave)
    {
        var config = await _repo.ObtenerPorClaveAsync(clave);
        if (config == null)
            return NotFound();

        return Ok(new ConfiguracionDto(config));
    }

    /// <summary>
    /// Obtiene solo el valor de una configuracion por clave
    /// </summary>
    [HttpGet("valor/{clave}")]
    public async Task<ActionResult<string>> GetValor(string clave)
    {
        var valor = await _repo.ObtenerValorAsync(clave);
        if (valor == null)
            return NotFound();

        return Ok(valor);
    }

    // =============================================================================
    // ESCRITURA
    // =============================================================================

    /// <summary>
    /// Actualiza el valor de una configuracion
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] ActualizarConfiguracionRequest request)
    {
        var config = await _repo.ObtenerPorIdAsync(id);
        if (config == null)
            return NotFound();

        config.ActualizarValor(request.Valor);
        await _repo.ActualizarAsync(config);

        _logger.LogInformation("Configuracion actualizada: {Clave} = {Valor}", config.Clave, request.Valor);
        return Ok();
    }

    /// <summary>
    /// Actualiza multiples configuraciones en batch
    /// </summary>
    [HttpPut("batch")]
    public async Task<ActionResult> UpdateBatch([FromBody] IEnumerable<ActualizarBatchItem> items)
    {
        var actualizaciones = items.Select(i => (i.Id, i.Valor));
        await _repo.ActualizarBatchAsync(actualizaciones);

        _logger.LogInformation("Configuraciones actualizadas en batch: {Count} items", items.Count());
        return Ok();
    }

    /// <summary>
    /// Crea una nueva configuracion (solo para uso interno/admin avanzado)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CrearConfiguracionRequest request)
    {
        var existente = await _repo.ObtenerPorClaveAsync(request.Clave);
        if (existente != null)
            return BadRequest("Ya existe una configuracion con esa clave");

        var config = new ConfiguracionSitio(
            request.Clave,
            request.Valor,
            request.Tipo,
            request.Grupo,
            request.Descripcion
        );

        var id = await _repo.CrearAsync(config);

        _logger.LogInformation("Configuracion creada: {Clave} (ID: {Id})", request.Clave, id);
        return Ok(id);
    }

    /// <summary>
    /// Desactiva una configuracion
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var config = await _repo.ObtenerPorIdAsync(id);
        if (config == null)
            return NotFound();

        config.Desactivar();
        await _repo.ActualizarAsync(config);

        _logger.LogInformation("Configuracion desactivada: {Clave} (ID: {Id})", config.Clave, id);
        return Ok();
    }
}

// =============================================================================
// DTOs
// =============================================================================

public record ConfiguracionDto(
    int Id,
    string Clave,
    string Valor,
    string Tipo,
    string Grupo,
    int Orden,
    string? Descripcion,
    bool Activo,
    DateTime FechaCreacion,
    DateTime? FechaActualizacion
)
{
    public ConfiguracionDto(ConfiguracionSitio c) : this(
        c.Id,
        c.Clave,
        c.Valor,
        c.Tipo,
        c.Grupo,
        c.Orden,
        c.Descripcion,
        c.Activo,
        c.FechaCreacion,
        c.FechaActualizacion
    )
    { }
}

public record ActualizarConfiguracionRequest(string Valor);

public record ActualizarBatchItem(int Id, string Valor);

public record CrearConfiguracionRequest(
    string Clave,
    string Valor,
    string Tipo = "string",
    string Grupo = "general",
    string? Descripcion = null
);
