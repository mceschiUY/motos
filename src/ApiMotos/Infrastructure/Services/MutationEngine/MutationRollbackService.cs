using FluentResults;
using Microsoft.Extensions.Logging;
using ApiMotos.Application.Agregates.Mutation.Commands.RevertirMutacion;
using ApiMotos.Domain.Agregates.Mutation;

namespace ApiMotos.Infrastructure.Services.MutationEngine;

/// <summary>
/// Servicio que revierte los cambios de una mutación ejecutada.
/// Restaura los archivos a su estado anterior usando el backup guardado.
/// </summary>
public class MutationRollbackService : IMutationRollbackService
{
    private readonly ILogger<MutationRollbackService> _logger;

    public MutationRollbackService(ILogger<MutationRollbackService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<bool>> RevertirAsync(
        IEnumerable<MutacionArchivo> archivos,
        CancellationToken cancellationToken = default)
    {
        var errores = new List<string>();

        // Procesar en orden inverso para deshacer correctamente
        foreach (var archivo in archivos.OrderByDescending(a => a.FechaOperacion))
        {
            try
            {
                await RevertirArchivoAsync(archivo);
            }
            catch (Exception ex)
            {
                var error = $"Error revirtiendo {archivo.RutaRelativa}: {ex.Message}";
                _logger.LogError(ex, "Error revirtiendo archivo {Ruta}", archivo.RutaCompleta);
                errores.Add(error);
            }
        }

        if (errores.Any())
        {
            return Result.Fail<bool>(string.Join("; ", errores));
        }

        return Result.Ok(true);
    }

    private async Task RevertirArchivoAsync(MutacionArchivo archivo)
    {
        switch (archivo.Operacion)
        {
            case TipoImpacto.Crear:
                // Si fue creado, eliminarlo
                if (File.Exists(archivo.RutaCompleta))
                {
                    File.Delete(archivo.RutaCompleta);
                    _logger.LogInformation("Rollback: Archivo eliminado (fue creado): {Ruta}", archivo.RutaCompleta);

                    // Limpiar directorios vacíos
                    LimpiarDirectoriosVacios(Path.GetDirectoryName(archivo.RutaCompleta));
                }
                break;

            case TipoImpacto.Modificar:
                // Restaurar contenido original
                if (!string.IsNullOrEmpty(archivo.ContenidoOriginal))
                {
                    await File.WriteAllTextAsync(archivo.RutaCompleta, archivo.ContenidoOriginal);
                    _logger.LogInformation("Rollback: Archivo restaurado: {Ruta}", archivo.RutaCompleta);
                }
                break;

            case TipoImpacto.Eliminar:
                // Recrear archivo eliminado
                if (!string.IsNullOrEmpty(archivo.ContenidoOriginal))
                {
                    var directorio = Path.GetDirectoryName(archivo.RutaCompleta);
                    if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
                    {
                        Directory.CreateDirectory(directorio);
                    }

                    await File.WriteAllTextAsync(archivo.RutaCompleta, archivo.ContenidoOriginal);
                    _logger.LogInformation("Rollback: Archivo recreado (fue eliminado): {Ruta}", archivo.RutaCompleta);
                }
                break;
        }
    }

    private void LimpiarDirectoriosVacios(string? directorio)
    {
        if (string.IsNullOrEmpty(directorio) || !Directory.Exists(directorio))
            return;

        try
        {
            // Solo eliminar si está vacío
            if (!Directory.EnumerateFileSystemEntries(directorio).Any())
            {
                Directory.Delete(directorio);
                _logger.LogInformation("Rollback: Directorio vacío eliminado: {Dir}", directorio);

                // Recursivamente limpiar padre
                LimpiarDirectoriosVacios(Path.GetDirectoryName(directorio));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo limpiar directorio: {Dir}", directorio);
        }
    }
}
