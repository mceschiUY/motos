using MediatR;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ApiMotos.Application.Agregates.Documentos.Queries.Documentos;

namespace ApiMotos.Application.Agregates.Documentos.Queries.ByRelacion
{
    public class DocumentosByRelacionHandler : IRequestHandler<DocumentosByRelacionQuery, List<DocumentosDto>>
    {
        private readonly string _connectionString;

        public DocumentosByRelacionHandler(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public async Task<List<DocumentosDto>> Handle(DocumentosByRelacionQuery query, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
SELECT id, nombre, extension, mimetype, fechacarga, relacionid, relacionnombre
FROM PC_DOCUMENTOS
WHERE relacionid = @RelacionId AND relacionnombre = @RelacionNombre
ORDER BY fechacarga DESC";

            var items = await connection.QueryAsync<DocumentosDto>(sql, new {
                RelacionId = query.RelacionId,
                RelacionNombre = query.RelacionNombre
            });
            return items.ToList();
        }
    }
}
