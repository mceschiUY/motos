namespace ApiMotos.Application.Common.Abstractions;

/// <summary>
/// Puerto de LECTURAS (Refactorización Ola 2, §4.3): los query handlers dejan de abrir
/// SqlConnection — la ejecución vive en el adapter de Infrastructure (DapperQueryService).
/// El SQL sigue siendo const emitido por el generador en el shell del handler; lo que
/// cruza el puerto es la EJECUCIÓN (conexión, Dapper), no la construcción del SQL.
/// </summary>
public interface IQueryService
{
    Task<List<T>> ConsultarAsync<T>(string sql, object? parametros = null);
    Task<int> EscalarAsync(string sql, object? parametros = null);
}
