using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Application.Agregates.Vendedores.Queries.Vendedores;

namespace ApiMotos.Application.Agregates.Vendedores.Queries.PorUsuario
{
    public class VendedorPorUsuarioHandler : IRequestHandler<VendedorPorUsuarioQuery, VendedoresDto?>
    {
        private const string Sql = @"
SELECT TOP 1 e.*
FROM PC_VENDEDORES e
WHERE e.Usuario IS NOT NULL AND LOWER(e.Usuario) = LOWER(@Usuario)
ORDER BY e.Id";

        private readonly IQueryService _consultas;

        public VendedorPorUsuarioHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<VendedoresDto?> Handle(VendedorPorUsuarioQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query.Usuario)) return null;
            var lista = await _consultas.ConsultarAsync<VendedoresDto>(Sql, new { Usuario = query.Usuario.Trim() });
            return lista.FirstOrDefault();
        }
    }
}
