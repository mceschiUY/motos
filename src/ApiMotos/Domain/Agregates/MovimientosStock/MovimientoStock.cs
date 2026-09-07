using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.MovimientosStock
{
    public class MovimientoStock : BaseEntity<int>
    {
        public static readonly string[] TiposValidos = { "entrada", "salida", "ajuste", "transferencia" };

        private MovimientoStock() : base()
        {
            Tipo = string.Empty;
            Fecha = Clock.Current.Now;
        }

        private MovimientoStock(int pVarianteId, int pDepositoId, int? pDepositoDestinoId, string pTipo,
            decimal pCantidad, decimal? pCostoUnitario, string? pMotivo, string? pDocumentoOrigen,
            DateTime pFecha, string? pUsuario)
        {
            VarianteId = pVarianteId;
            DepositoId = pDepositoId;
            DepositoDestinoId = pDepositoDestinoId;
            Tipo = pTipo;
            Cantidad = pCantidad;
            CostoUnitario = pCostoUnitario;
            Motivo = pMotivo;
            DocumentoOrigen = pDocumentoOrigen;
            Fecha = pFecha;
            Usuario = pUsuario;
        }

        public int VarianteId { get; private set; }
        public int DepositoId { get; private set; }
        public int? DepositoDestinoId { get; private set; }
        public string Tipo { get; private set; }
        public decimal Cantidad { get; private set; }
        public decimal? CostoUnitario { get; private set; }
        public string? Motivo { get; private set; }
        public string? DocumentoOrigen { get; private set; }
        public DateTime Fecha { get; private set; }
        public string? Usuario { get; private set; }

        private static Result Validar(int varianteId, int depositoId, int? depositoDestinoId, string tipo, decimal cantidad)
        {
            if (varianteId <= 0) return Result.Fail("Variante es requerida");
            if (depositoId <= 0) return Result.Fail("Deposito es requerido");
            if (string.IsNullOrWhiteSpace(tipo) || Array.IndexOf(TiposValidos, tipo) < 0)
                return Result.Fail("Tipo inválido (entrada, salida, ajuste o transferencia)");
            if (cantidad <= 0) return Result.Fail("La cantidad debe ser mayor a cero");
            if (tipo == "transferencia")
            {
                if (!depositoDestinoId.HasValue) return Result.Fail("La transferencia requiere un depósito destino");
                if (depositoDestinoId.Value == depositoId) return Result.Fail("El depósito destino debe ser distinto del origen");
            }
            else if (depositoDestinoId.HasValue)
            {
                return Result.Fail("El depósito destino solo aplica a transferencias");
            }
            return Result.Ok();
        }

        public static Result<MovimientoStock> Crear(int varianteId, int depositoId, int? depositoDestinoId, string tipo,
            decimal cantidad, decimal? costoUnitario, string? motivo, string? documentoOrigen, DateTime? fecha, string? usuario)
        {
            var v = Validar(varianteId, depositoId, depositoDestinoId, tipo, cantidad);
            if (v.IsFailed) return Result.Fail<MovimientoStock>(v.Errors);
            return new MovimientoStock(varianteId, depositoId, depositoDestinoId, tipo, cantidad, costoUnitario,
                motivo, documentoOrigen, fecha ?? Clock.Current.Now, usuario);
        }

        public Result<MovimientoStock> Modificar(int pVarianteId, int pDepositoId, int? pDepositoDestinoId, string pTipo,
            decimal pCantidad, decimal? pCostoUnitario, string? pMotivo, string? pDocumentoOrigen, DateTime? pFecha, string? pUsuario)
        {
            var v = Validar(pVarianteId, pDepositoId, pDepositoDestinoId, pTipo, pCantidad);
            if (v.IsFailed) return Result.Fail<MovimientoStock>(v.Errors);
            VarianteId = pVarianteId;
            DepositoId = pDepositoId;
            DepositoDestinoId = pDepositoDestinoId;
            Tipo = pTipo;
            Cantidad = pCantidad;
            CostoUnitario = pCostoUnitario;
            Motivo = pMotivo;
            DocumentoOrigen = pDocumentoOrigen;
            Fecha = pFecha ?? Fecha;
            Usuario = pUsuario;
            return this;
        }
    }
}
