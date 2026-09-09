using System.Text.RegularExpressions;
using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Metas
{
    /// <summary>
    /// Meta mensual por vendedor (plan §3.8 / §4.8): objetivo en USD para un período `YYYY-MM`.
    /// Única por (VendedorId, Periodo) — la unicidad la exige MetaHooks (mensaje claro) y la
    /// respalda el índice único de PC_METAS.
    /// </summary>
    public class Meta : BaseEntity<int>
    {
        public static readonly Regex PeriodoValido = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);

        private Meta() : base()
        {
            Periodo = string.Empty;
        }

        private Meta(int pVendedorId, string pPeriodo, decimal pObjetivoUsd)
        {
            VendedorId = pVendedorId;
            Periodo = pPeriodo;
            ObjetivoUsd = pObjetivoUsd;
        }

        public int VendedorId { get; private set; }
        /// <summary>Año-mes `YYYY-MM`.</summary>
        public string Periodo { get; private set; }
        public decimal ObjetivoUsd { get; private set; }

        private static Result Validar(int vendedorId, string periodo, decimal objetivoUsd)
        {
            if (vendedorId <= 0) return Result.Fail("Vendedor es requerido");
            if (string.IsNullOrWhiteSpace(periodo) || !PeriodoValido.IsMatch(periodo.Trim()))
                return Result.Fail("Periodo inválido: debe tener el formato YYYY-MM (ej. 2026-09)");
            if (objetivoUsd <= 0) return Result.Fail("ObjetivoUsd debe ser mayor que cero");
            return Result.Ok();
        }

        public static Result<Meta> Crear(int vendedorId, string periodo, decimal objetivoUsd)
        {
            var v = Validar(vendedorId, periodo, objetivoUsd);
            if (v.IsFailed) return Result.Fail<Meta>(v.Errors);
            return new Meta(vendedorId, periodo.Trim(), objetivoUsd);
        }

        public Result<Meta> Modificar(int pVendedorId, string pPeriodo, decimal pObjetivoUsd)
        {
            var v = Validar(pVendedorId, pPeriodo, pObjetivoUsd);
            if (v.IsFailed) return Result.Fail<Meta>(v.Errors);
            VendedorId = pVendedorId;
            Periodo = pPeriodo.Trim();
            ObjetivoUsd = pObjetivoUsd;
            return this;
        }
    }
}
