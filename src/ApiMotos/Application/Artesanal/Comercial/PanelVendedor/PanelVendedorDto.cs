namespace ApiMotos.Application.Artesanal.Comercial.PanelVendedor
{
    /// <summary>
    /// Lo que la escena /vendedor/:id NO saca de los endpoints que ya existen (avance, agenda,
    /// comisiones): cabecera del vendedor, cartera con semáforo, pedidos abiertos, comisión
    /// proyectada, ventas por semana y ranking del mes. Solo USD.
    /// </summary>
    public class PanelVendedorDto
    {
        public PanelVendedorCabeceraDto Vendedor { get; set; } = new();
        public List<PanelVendedorClienteDto> Clientes { get; set; } = new();
        public List<PanelVendedorPedidoDto> PedidosAbiertos { get; set; } = new();
        /// <summary>Total de los pedidos del mes no anulados y todavía no entregados × % de comisión del vendedor.</summary>
        public decimal ComisionProyectadaUsd { get; set; }
        /// <summary>Días del parámetro `crm.dias_sin_visita` usado para el semáforo de los clientes.</summary>
        public int DiasSinVisitaUmbral { get; set; }
        /// <summary>Base de la proyección (total USD de esos pedidos), para mostrar "si entrega lo que tiene abierto".</summary>
        public decimal PendienteEntregaUsd { get; set; }
        /// <summary>Últimas 8 semanas (lunes a domingo), la más vieja primero. Siempre 8 filas.</summary>
        public List<PanelVendedorSemanaDto> VentasPorSemana { get; set; } = new();
        public PanelVendedorRankingDto Ranking { get; set; } = new();
    }

    public class PanelVendedorCabeceraDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Zona { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public decimal ComisionPorcentaje { get; set; }
        public bool Activo { get; set; }
        public string? Usuario { get; set; }
    }

    public class PanelVendedorClienteDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Tipo { get; set; }
        public string? Ciudad { get; set; }
        /// <summary>Días desde la última actividad registrada con el cliente; null si nunca.</summary>
        public int? DiasSinVisita { get; set; }
        public DateTime? UltimoPedidoFecha { get; set; }
        /// <summary>Pedidos del año calendario (distintos de borrador/anulado).</summary>
        public int PedidosAnio { get; set; }
        public decimal TotalAnioUsd { get; set; }
        /// <summary>verde (hasta 15 días) | amarillo (16 a 30) | rojo (más de 30 o nunca).</summary>
        public string Semaforo { get; set; } = "rojo";
    }

    public class PanelVendedorPedidoDto
    {
        public int Id { get; set; }
        public string? Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string? Estado { get; set; }
        public string? ClienteNombre { get; set; }
        public decimal TotalUsd { get; set; }
    }

    public class PanelVendedorSemanaDto
    {
        /// <summary>Lunes de la semana (solo fecha).</summary>
        public DateTime SemanaInicio { get; set; }
        public decimal TotalUsd { get; set; }
        public int Pedidos { get; set; }
    }

    public class PanelVendedorRankingDto
    {
        /// <summary>1 = el que más vendió (entregado) en el mes. 0 si no rankea.</summary>
        public int Posicion { get; set; }
        /// <summary>Vendedores activos considerados.</summary>
        public int Total { get; set; }
    }
}
