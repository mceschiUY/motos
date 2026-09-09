using System.Globalization;
using MediatR;
using ApiMotos.Application.Common.Abstractions;
using ApiMotos.Domain.Agregates.Metas;

namespace ApiMotos.Application.Artesanal.Comercial.Comisiones
{
    /// <summary>
    /// READ-ONLY (zona artesanal). Liquidación por vendedor y mes: se suman los pedidos
    /// ENTREGADOS del período con la comisión que cada uno selló al entregarse — no se
    /// recalcula con el porcentaje de hoy, porque el vendedor cobró lo de aquel momento.
    /// Aparecen todos los vendedores activos, aunque no hayan vendido (fila en cero).
    /// </summary>
    public class ComisionesHandler : IRequestHandler<ComisionesQuery, List<ComisionVendedorDto>>
    {
        private const string Sql = @"
SELECT v.Id AS VendedorId
     , v.Nombre AS VendedorDisplay
     , v.Zona AS Zona
     , v.ComisionPorcentaje AS ComisionPorcentaje
     , @Periodo AS Periodo
     , ISNULL(p.Pedidos, 0) AS Pedidos
     , ISNULL(p.TotalUsd, 0) AS TotalUsd
     , ISNULL(p.ComisionUsd, 0) AS ComisionUsd
FROM PC_VENDEDORES v
LEFT JOIN (
    SELECT VendedorId, COUNT(*) AS Pedidos, SUM(TotalUsd) AS TotalUsd, SUM(ComisionUsd) AS ComisionUsd
    FROM PC_PEDIDOS
    WHERE Estado = N'entregado' AND Fecha >= @Desde AND Fecha < @Hasta
    GROUP BY VendedorId
) p ON p.VendedorId = v.Id
WHERE (@VendedorId IS NULL OR v.Id = @VendedorId)
  AND (v.Activo = 1 OR ISNULL(p.Pedidos, 0) > 0)
ORDER BY ISNULL(p.ComisionUsd, 0) DESC, v.Nombre";

        private readonly IQueryService _consultas;

        public ComisionesHandler(IQueryService consultas)
        {
            _consultas = consultas;
        }

        public Task<List<ComisionVendedorDto>> Handle(ComisionesQuery query, CancellationToken cancellationToken)
        {
            var hoy = ApiMotos.Domain.Common.Clock.Current.Today;
            var periodo = string.IsNullOrWhiteSpace(query.Periodo) ? hoy.ToString("yyyy-MM") : query.Periodo.Trim();
            if (!Meta.PeriodoValido.IsMatch(periodo))
                throw new ArgumentException("Periodo inválido: debe tener el formato YYYY-MM (ej. 2026-09)");

            var desde = DateTime.ParseExact(periodo + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return _consultas.ConsultarAsync<ComisionVendedorDto>(Sql, new
            {
                Periodo = periodo,
                Desde = desde,
                Hasta = desde.AddMonths(1),
                VendedorId = query.VendedorId,
            });
        }
    }
}
