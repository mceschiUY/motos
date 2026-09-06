using ApiMotos.Domain.Common;

namespace ApiMotos.Application.Agregates.Envios.Queries.Envios
{
    public class EnviosDto
    {
        public int Id { get; set; }
        public string CodigoRastreo { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRecibido { get; set; }
        public DateTime FechaFactura { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string MotivoAnulacion { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteDisplay { get; set; }
        public int AgenciaId { get; set; }
        public string? AgenciaDisplay { get; set; }

        // R-009 [calculo]: Días de demora de facturación = fechaFactura - fechaRecibido.
        // Para envíos sin facturar aún (fechaFactura no posterior a fechaRecibido) se mide
        // contra la fecha actual. Campo derivado read-only: se serializa en la respuesta GET
        // sin necesidad de tocar el SQL (Dapper mapea e.*; esta propiedad no tiene columna).
        public int DemoraFacturacion
        {
            get
            {
                var hasta = FechaFactura > FechaRecibido ? FechaFactura : Clock.Current.Now;
                return (int)(hasta.Date - FechaRecibido.Date).TotalDays;
            }
        }

        // R-010 [calculo]: Días de atraso de despacho = fechaEnvio - fechaFactura.
        // Para envíos facturados sin despachar aún (fechaEnvio no posterior a fechaFactura)
        // se mide contra la fecha actual. Campo derivado read-only: se serializa en la
        // respuesta GET sin tocar el SQL (Dapper mapea e.*; esta propiedad no tiene columna).
        public int AtrasoDespacho
        {
            get
            {
                var hasta = FechaEnvio > FechaFactura ? FechaEnvio : Clock.Current.Now;
                return (int)(hasta.Date - FechaFactura.Date).TotalDays;
            }
        }

        // R-011 [calculo]: Días de demora de agencia = fechaEntrega - fechaEnvio.
        // Para envíos despachados sin entregar aún (fechaEntrega no posterior a fechaEnvio)
        // se mide contra la fecha actual. Campo derivado read-only: se serializa en la
        // respuesta GET sin tocar el SQL (Dapper mapea e.*; esta propiedad no tiene columna).
        public int DemoraAgencia
        {
            get
            {
                var hasta = FechaEntrega > FechaEnvio ? FechaEntrega : Clock.Current.Now;
                return (int)(hasta.Date - FechaEnvio.Date).TotalDays;
            }
        }

        // R-012 [calculo]: Tiempo de ciclo total = fechaEntrega - fechaRecibido.
        // Para envíos no entregados aún (fechaEntrega no posterior a fechaRecibido) se mide
        // contra la fecha actual. Campo derivado read-only: se serializa en la respuesta GET
        // sin tocar el SQL (Dapper mapea e.*; esta propiedad no tiene columna).
        public int TiempoCicloTotal
        {
            get
            {
                var hasta = FechaEntrega > FechaRecibido ? FechaEntrega : Clock.Current.Now;
                return (int)(hasta.Date - FechaRecibido.Date).TotalDays;
            }
        }
    }
}
