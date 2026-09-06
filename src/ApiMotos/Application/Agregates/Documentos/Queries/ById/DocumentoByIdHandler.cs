using MediatR;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiMotos.Application.Agregates.Documentos.Queries.Documentos;

namespace ApiMotos.Application.Agregates.Documentos.Queries.ById
{
    public class DocumentoByIdHandler : IRequestHandler<DocumentoByIdQuery, DocumentoContenidoDto?>
    {
        private readonly string _connectionString;

        public DocumentoByIdHandler(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<DocumentoContenidoDto?> Handle(DocumentoByIdQuery query, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);

            // Incluimos Contenido para descarga
            var sql = @"
SELECT id, nombre, extension, contenido, mimetype, fechacarga, relacionid, relacionnombre
FROM PC_DOCUMENTOS
WHERE id = @Id";

            var item = await connection.QueryFirstOrDefaultAsync<DocumentoContenidoDto>(sql, new { Id = query.Id });
            return item;
        }
    }
}
