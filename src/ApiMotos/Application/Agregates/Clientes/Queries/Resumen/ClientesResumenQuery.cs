using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Clientes.Queries.Resumen
{
    public record ClientesResumenQuery() : IQuery<ClientesResumenDto>;

    public class ClientesResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoCliente> PorEstado { get; set; } = new();
        public List<GrupoConteoCliente> PorMes { get; set; } = new();
    }

    public class GrupoConteoCliente
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
