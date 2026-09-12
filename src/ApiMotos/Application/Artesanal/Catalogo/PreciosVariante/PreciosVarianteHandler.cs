using MediatR;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Application.Artesanal.Catalogo.PreciosVariante
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Lee PC_PRECIO_HISTORIAL, que solo escribe VarianteHooks.
    /// La diferencia y el porcentaje se calculan acá, no en SQL, para no repetir el CASE del /0.
    /// </summary>
    public class PreciosVarianteHandler : IRequestHandler<PreciosVarianteQuery, List<PrecioVarianteDto>>
    {
        private const string Sql = @"
SELECT h.Id, h.VarianteId, h.Campo, h.ValorAnterior, h.ValorNuevo, h.Fecha, h.Usuario
FROM PC_PRECIO_HISTORIAL h
WHERE h.VarianteId = @VarianteId
ORDER BY h.Fecha DESC, h.Id DESC";

        private readonly IQueryService _consultas;

        public PreciosVarianteHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public async Task<List<PrecioVarianteDto>> Handle(PreciosVarianteQuery query, CancellationToken cancellationToken)
        {
            var filas = await _consultas.ConsultarAsync<PrecioVarianteDto>(Sql, new { query.VarianteId });
            foreach (var f in filas)
            {
                f.DiferenciaUsd = Math.Round(f.ValorNuevo - f.ValorAnterior, 2, MidpointRounding.AwayFromZero);
                f.DiferenciaPorcentaje = f.ValorAnterior == 0m
                    ? 0m
                    : Math.Round((f.ValorNuevo - f.ValorAnterior) / f.ValorAnterior * 100m, 2, MidpointRounding.AwayFromZero);
            }
            return filas;
        }
    }
}
