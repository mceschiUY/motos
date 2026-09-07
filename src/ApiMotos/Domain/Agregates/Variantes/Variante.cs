using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Variantes
{
    public class Variante : BaseEntity<int>
    {
        private Variante() : base()
        {
            Sku = string.Empty;
        }

        private Variante(int pProductoId, int? pTallaId, int? pColorId, string pSku, string? pCodigoBarras,
            decimal pCostoEstandar, decimal pPrecioLista, bool pActivo)
        {
            ProductoId = pProductoId;
            TallaId = pTallaId;
            ColorId = pColorId;
            Sku = pSku;
            CodigoBarras = pCodigoBarras;
            CostoEstandar = pCostoEstandar;
            PrecioLista = pPrecioLista;
            Activo = pActivo;
        }

        public int ProductoId { get; private set; }
        public int? TallaId { get; private set; }
        public int? ColorId { get; private set; }
        public string Sku { get; private set; }
        public string? CodigoBarras { get; private set; }
        public decimal CostoEstandar { get; private set; }
        public decimal PrecioLista { get; private set; }
        public bool Activo { get; private set; }

        public static Result<Variante> Crear(int productoId, int? tallaId, int? colorId, string sku, string? codigoBarras,
            decimal costoEstandar, decimal precioLista, bool activo)
        {
            if (productoId <= 0) return Result.Fail<Variante>("Producto es requerido");
            if (string.IsNullOrWhiteSpace(sku)) return Result.Fail<Variante>("Sku es requerido");
            if (costoEstandar < 0) return Result.Fail<Variante>("El costo no puede ser negativo");
            if (precioLista < 0) return Result.Fail<Variante>("El precio no puede ser negativo");
            return new Variante(productoId, tallaId, colorId, sku, codigoBarras, costoEstandar, precioLista, activo);
        }

        public Result<Variante> Modificar(int pProductoId, int? pTallaId, int? pColorId, string pSku, string? pCodigoBarras,
            decimal pCostoEstandar, decimal pPrecioLista, bool pActivo)
        {
            if (pProductoId <= 0) return Result.Fail<Variante>("Producto es requerido");
            if (string.IsNullOrWhiteSpace(pSku)) return Result.Fail<Variante>("Sku es requerido");
            if (pCostoEstandar < 0) return Result.Fail<Variante>("El costo no puede ser negativo");
            if (pPrecioLista < 0) return Result.Fail<Variante>("El precio no puede ser negativo");
            ProductoId = pProductoId;
            TallaId = pTallaId;
            ColorId = pColorId;
            Sku = pSku;
            CodigoBarras = pCodigoBarras;
            CostoEstandar = pCostoEstandar;
            PrecioLista = pPrecioLista;
            Activo = pActivo;
            return this;
        }
    }
}
