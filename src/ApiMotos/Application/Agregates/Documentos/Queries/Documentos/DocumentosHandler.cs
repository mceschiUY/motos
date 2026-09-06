using MediatR;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ApiMotos.Application.Agregates.Documentos.Queries.Documentos
{
    public class DocumentosHandler : IRequestHandler<DocumentosQuery, List<DocumentosDto>>
    {
        private readonly string _connectionString;

        public DocumentosHandler(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<List<DocumentosDto>> Handle(DocumentosQuery query, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);

            // No traemos Contenido para optimizar la lista
            var sql = @"
SELECT id, nombre, extension, mimetype, fechacarga, relacionid, relacionnombre
FROM PC_DOCUMENTOS
ORDER BY fechacarga DESC";

            var items = await connection.QueryAsync<DocumentosDto>(sql);
            return items.ToList();
        }
    }
}
