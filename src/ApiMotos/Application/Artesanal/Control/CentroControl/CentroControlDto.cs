namespace ApiMotos.Application.Artesanal.Control.CentroControl
{
    /// <summary>Respuesta única del home "Hoy". Todo en USD. Fechas locales (Clock.Current).</summary>
    public class CentroControlDto
    {
        public DateTime Fecha { get; set; }
        /// <summary>Período del mes en curso, `YYYY-MM` (el de PC_METAS).</summary>
        public string Periodo { get; set; } = "";
        /// <summary>Umbral de "stock bajo" (Cfg_ConfiguracionSitio `stock.umbral_bajo`, default 3).</summary>
        public decimal UmbralStockBajo { get; set; }
        /// <summary>Días sin actividad a partir de los cuales un cliente entra en alerta (`crm.dias_sin_visita`, default 30).</summary>
        public int DiasSinVisita { get; set; }
        public CentroControlKpisDto Kpis { get; set; } = new();
        /// <summary>El feed: qué hacer hoy, ordenado por urgencia (máximo 12).</summary>
        public List<AccionDto> Acciones { get; set; } = new();
        /// <summary>Pedidos del mes por estado (borrador → entregado; sin anulados).</summary>
        public List<PipelineEstadoDto> Pipeline { get; set; } = new();
        /// <summary>Vendedores activos contra su meta del período, ordenados por vendido.</summary>
        public List<VendedorResumenDto> Vendedores { get; set; } = new();
        /// <summary>Top 5 productos del mes por facturado (líneas de pedidos no anulados).</summary>
        public List<TopProductoDto> TopProductos { get; set; } = new();
        public List<StockDepositoDto> StockPorDeposito { get; set; } = new();
    }

    public class CentroControlKpisDto
    {
        /// <summary>Pedidos con Fecha = hoy (sin anulados).</summary>
        public int PedidosNuevosHoy { get; set; }
        /// <summary>Pedidos en `confirmado` + `preparado`: los que esperan salir del depósito.</summary>
        public int PedidosADespachar { get; set; }
        /// <summary>Paradas de la agenda de hoy: actividades con ProximaAccion = hoy o Fecha = hoy (regla de Agenda).</summary>
        public int VisitasHoy { get; set; }
        /// <summary>Envíos en curso en advertencia o vencidos según PC_PARAMETROSLAS.</summary>
        public int EnviosFueraSla { get; set; }
        public int EnviosSlaVencidos { get; set; }
        /// <summary>Pedidos del mes en estado distinto de borrador/anulado.</summary>
        public decimal VentasMesUsd { get; set; }
        public decimal VentasMesAnteriorUsd { get; set; }
        /// <summary>Variación de ventas contra el mes anterior, en %. Null si el mes anterior fue 0.</summary>
        public decimal? VariacionMesPorcentaje { get; set; }
        /// <summary>SKU activos con saldo total &gt; 0 y &lt; umbral.</summary>
        public int SkuStockBajo { get; set; }
        /// <summary>SKU con saldo total &lt; 0 (el Kardex quedó en negativo).</summary>
        public int SkuStockNegativo { get; set; }
        /// <summary>Pedidos ENTREGADOS del mes: la base de las comisiones y del "vendió" del equipo.</summary>
        public decimal VentasEntregadasMesUsd { get; set; }
        /// <summary>Comisión sellada en los pedidos entregados del mes.</summary>
        public decimal ComisionesMesUsd { get; set; }
    }

    /// <summary>Una fila del feed: qué pasa, qué hacer y a dónde ir.</summary>
    public class AccionDto
    {
        /// <summary>sla | despachar | visita | stock | meta | pedido_nuevo | sin_visita</summary>
        public string Tipo { get; set; } = "";
        /// <summary>danger | warning | primary | success</summary>
        public string Acento { get; set; } = "";
        /// <summary>Material Symbols.</summary>
        public string Icono { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Detalle { get; set; } = "";
        /// <summary>Ruta del front (ej. `/envio/12`, `/agenda?vendedorId=3`).</summary>
        public string Ruta { get; set; } = "";
        public DateTime? Fecha { get; set; }
    }

    public class PipelineEstadoDto
    {
        public string Estado { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal TotalUsd { get; set; }
    }

    public class VendedorResumenDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Zona { get; set; }
        /// <summary>Pedidos del mes del vendedor en estado distinto de borrador/anulado (misma regla que VentasMesUsd).</summary>
        public decimal VendidoMesUsd { get; set; }
        /// <summary>Meta del período (PC_METAS); 0 si no tiene.</summary>
        public decimal ObjetivoUsd { get; set; }
        /// <summary>Vendido / objetivo en %. 0 si no hay meta.</summary>
        public decimal AvancePorcentaje { get; set; }
        /// <summary>Comisión sellada en los pedidos entregados del mes.</summary>
        public decimal ComisionMesUsd { get; set; }
        /// <summary>Actividades de tipo visita en el mes.</summary>
        public int VisitasMes { get; set; }
    }

    public class TopProductoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = "";
        public string? Marca { get; set; }
        public decimal UnidadesMes { get; set; }
        public decimal TotalUsd { get; set; }
    }

    public class StockDepositoDto
    {
        public int DepositoId { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Unidades { get; set; }
    }
}
