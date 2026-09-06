using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiMotos.Application.Agregates.Mutation.Commands.EjecutarMutacion;
using ApiMotos.Application.Agregates.Mutation.DTOs;
using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Infrastructure.Services.MutationEngine;

/// <summary>
/// Servicio que ejecuta físicamente los cambios de una mutación.
/// Escribe archivos, hace backup del contenido original y registra los cambios.
/// </summary>
public class MutationExecutorService : IMutationExecutorService
{
    private readonly IMutacionRepositorio _mutacionRepositorio;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MutationExecutorService> _logger;

    public MutationExecutorService(
        IMutacionRepositorio mutacionRepositorio,
        IConfiguration configuration,
        ILogger<MutationExecutorService> logger)
    {
        _mutacionRepositorio = mutacionRepositorio;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<EjecucionResultadoDto>> EjecutarMutacionAsync(
        Mutacion mutacion,
        CancellationToken cancellationToken = default)
    {
        var archivosAfectados = new List<string>();
        var archivosCreados = 0;
        var archivosModificados = 0;
        var archivosEliminados = 0;

        var basePath = _configuration["CodeGenerator:BasePath"] ?? ".";
        var sitePath = _configuration["CodeGenerator:SiteProjectPath"] ?? "../SiteMotos";

        _logger.LogInformation("=== EJECUTANDO MUTACIÓN {Id} ===", mutacion.Id);
        _logger.LogInformation("BasePath: {BasePath}", Path.GetFullPath(basePath));
        _logger.LogInformation("SitePath: {SitePath}", Path.GetFullPath(sitePath));
        _logger.LogInformation("Total Impactos: {Count}", mutacion.Impactos?.Count ?? 0);

        try
        {
            if (mutacion.Impactos == null || !mutacion.Impactos.Any())
            {
                _logger.LogWarning("¡No hay impactos para ejecutar!");
                return Result.Fail<EjecucionResultadoDto>("No hay impactos para ejecutar");
            }

            foreach (var impacto in mutacion.Impactos.OrderBy(i => i.Orden))
            {
                _logger.LogInformation("Impacto {Orden}: Capa={Capa}, Tipo={Tipo}, Ruta={Ruta}, TieneCodigo={TieneCodigo}",
                    impacto.Orden, impacto.Capa, impacto.Tipo,
                    impacto.RutaArchivo ?? "(null)",
                    !string.IsNullOrEmpty(impacto.CodigoGenerado));

                if (string.IsNullOrEmpty(impacto.RutaArchivo) || string.IsNullOrEmpty(impacto.CodigoGenerado))
                {
                    _logger.LogWarning("Saltando impacto - RutaArchivo o CodigoGenerado vacío");
                    continue;
                }

                // Determinar ruta base según la capa
                var projectBasePath = impacto.Capa == CapaArquitectura.Presentation
                    ? Path.GetFullPath(sitePath)
                    : Path.GetFullPath(basePath);

                var rutaCompleta = Path.Combine(projectBasePath, impacto.RutaArchivo);
                var rutaRelativa = impacto.RutaArchivo;

                _logger.LogInformation("Procesando impacto: {Tipo} {Ruta}", impacto.Tipo, rutaCompleta);

                switch (impacto.Tipo)
                {
                    case TipoImpacto.Crear:
                        await CrearArchivoAsync(mutacion.Id, rutaCompleta, rutaRelativa, impacto.CodigoGenerado);
                        archivosCreados++;
                        archivosAfectados.Add(rutaRelativa);
                        break;

                    case TipoImpacto.Modificar:
                        await ModificarArchivoAsync(mutacion.Id, rutaCompleta, rutaRelativa, impacto.CodigoGenerado);
                        archivosModificados++;
                        archivosAfectados.Add(rutaRelativa);
                        break;

                    case TipoImpacto.Eliminar:
                        await EliminarArchivoAsync(mutacion.Id, rutaCompleta, rutaRelativa);
                        archivosEliminados++;
                        archivosAfectados.Add(rutaRelativa);
                        break;
                }
            }

            return Result.Ok(new EjecucionResultadoDto
            {
                Success = true,
                MutacionId = mutacion.Id,
                ArchivosCreados = archivosCreados,
                ArchivosModificados = archivosModificados,
                ArchivosEliminados = archivosEliminados,
                ArchivosAfectados = archivosAfectados
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando mutación {MutacionId}", mutacion.Id);
            return Result.Fail<EjecucionResultadoDto>($"Error al ejecutar mutación: {ex.Message}");
        }
    }

    private async Task CrearArchivoAsync(int mutacionId, string rutaCompleta, string rutaRelativa, string contenido)
    {
        // Crear directorio si no existe
        var directorio = Path.GetDirectoryName(rutaCompleta);
        if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
        {
            Directory.CreateDirectory(directorio);
            _logger.LogInformation("Directorio creado: {Directorio}", directorio);
        }

        // Verificar si el archivo ya existe (sería una modificación, no creación)
        if (File.Exists(rutaCompleta))
        {
            _logger.LogWarning("Archivo ya existe, se sobrescribirá: {Ruta}", rutaCompleta);
            await ModificarArchivoAsync(mutacionId, rutaCompleta, rutaRelativa, contenido);
            return;
        }

        // Escribir archivo
        await File.WriteAllTextAsync(rutaCompleta, contenido);
        _logger.LogInformation("Archivo creado: {Ruta}", rutaCompleta);

        // Registrar para rollback
        var archivo = MutacionArchivo.RegistrarCreacion(mutacionId, rutaCompleta, rutaRelativa, contenido);
        await _mutacionRepositorio.AgregarArchivoAsync(archivo);
    }

    private async Task ModificarArchivoAsync(int mutacionId, string rutaCompleta, string rutaRelativa, string contenidoNuevo)
    {
        // Leer contenido original
        var contenidoOriginal = File.Exists(rutaCompleta)
            ? await File.ReadAllTextAsync(rutaCompleta)
            : string.Empty;

        // Crear directorio si no existe
        var directorio = Path.GetDirectoryName(rutaCompleta);
        if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        // Escribir nuevo contenido
        await File.WriteAllTextAsync(rutaCompleta, contenidoNuevo);
        _logger.LogInformation("Archivo modificado: {Ruta}", rutaCompleta);

        // Registrar para rollback
        var archivo = MutacionArchivo.RegistrarModificacion(
            mutacionId, rutaCompleta, rutaRelativa, contenidoOriginal, contenidoNuevo);
        await _mutacionRepositorio.AgregarArchivoAsync(archivo);
    }

    private async Task EliminarArchivoAsync(int mutacionId, string rutaCompleta, string rutaRelativa)
    {
        if (!File.Exists(rutaCompleta))
        {
            _logger.LogWarning("Archivo a eliminar no existe: {Ruta}", rutaCompleta);
            return;
        }

        // Leer contenido original para backup
        var contenidoOriginal = await File.ReadAllTextAsync(rutaCompleta);

        // Eliminar archivo
        File.Delete(rutaCompleta);
        _logger.LogInformation("Archivo eliminado: {Ruta}", rutaCompleta);

        // Registrar para rollback
        var archivo = MutacionArchivo.RegistrarEliminacion(
            mutacionId, rutaCompleta, rutaRelativa, contenidoOriginal);
        await _mutacionRepositorio.AgregarArchivoAsync(archivo);
    }
}
