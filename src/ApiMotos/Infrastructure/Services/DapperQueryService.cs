using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Infrastructure.Services;

/// <summary>
/// Adapter Dapper del puerto IQueryService (Refactorización Ola 2): el ÚNICO lugar donde
/// las lecturas abren conexión. Application ya no conoce SqlConnection ni el connection
/// string — solo el puerto.
/// </summary>
public sealed class DapperQueryService : IQueryService
{
    private readonly string _connectionString;

    public DapperQueryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");
    }

    public async Task<List<T>> ConsultarAsync<T>(string sql, object? parametros = null)
    {
        using var connection = new SqlConnection(_connectionString);
        return (await connection.QueryAsync<T>(sql, parametros)).ToList();
    }

    public async Task<int> EscalarAsync(string sql, object? parametros = null)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(sql, parametros);
    }
}
