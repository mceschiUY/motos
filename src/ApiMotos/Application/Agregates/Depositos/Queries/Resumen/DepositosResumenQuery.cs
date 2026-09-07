using ApiMotos.Application.Common;

namespace ApiMotos.Application.Agregates.Depositos.Queries.Resumen
{
    public record DepositosResumenQuery() : IQuery<DepositosResumenDto>;

    public class DepositosResumenDto
    {
        public int Total { get; set; }
        public List<GrupoConteoDeposito> PorEstado { get; set; } = new();
        public List<GrupoConteoDeposito> PorMes { get; set; } = new();
    }

    public class GrupoConteoDeposito
    {
        public string Clave { get; set; } = "";
        public int Cantidad { get; set; }
    }
}
