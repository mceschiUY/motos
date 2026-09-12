using System.Globalization;
using ApiMotos.Application.Common.Abstractions;

namespace ApiMotos.Application.Artesanal.Comun
{
    /// <summary>
    /// Parámetros de las alertas del negocio, leídos de Configuración del sitio
    /// (`Cfg_ConfiguracionSitio`, grupo `alertas`; plan §3.8 y Etapa D):
    ///   · `stock.umbral_bajo`   — SKU con menos unidades que esto = "stock bajo" (default 3).
    ///   · `crm.dias_sin_visita` — cliente sin actividad hace más de N días = alerta (default 30).
    /// Los usan el centro de control, la agenda, el panel del vendedor y el cliente 360, para que
    /// todas las pantallas avisen con el mismo criterio. Si la tabla no existe o el valor no es
    /// un número válido, cae al default.
    /// </summary>
    public static class ParametrosAlertas
    {
        public const decimal UmbralStockBajoDefault = 3m;
        public const int DiasSinVisitaDefault = 30;

        private const string Sql = @"
SELECT TOP 1 Valor FROM Cfg_ConfiguracionSitio WHERE Clave = @Clave AND Activo = 1";

        private sealed class ValorFila { public string? Valor { get; set; } }

        public static async Task<decimal> UmbralStockBajoAsync(IQueryService consultas)
        {
            var v = await LeerAsync(consultas, "stock.umbral_bajo");
            return v.HasValue && v.Value > 0m ? v.Value : UmbralStockBajoDefault;
        }

        public static async Task<int> DiasSinVisitaAsync(IQueryService consultas)
        {
            var v = await LeerAsync(consultas, "crm.dias_sin_visita");
            return v.HasValue && v.Value > 0m ? (int)Math.Round(v.Value) : DiasSinVisitaDefault;
        }

        private static async Task<decimal?> LeerAsync(IQueryService consultas, string clave)
        {
            try
            {
                var fila = (await consultas.ConsultarAsync<ValorFila>(Sql, new { Clave = clave })).FirstOrDefault();
                if (fila?.Valor != null
                    && decimal.TryParse(fila.Valor.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
                    return v;
            }
            catch
            {
                // La tabla de configuración puede no existir todavía (primer arranque): default.
            }
            return null;
        }
    }
}
