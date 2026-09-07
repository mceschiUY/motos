using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Productos
{
    public class Producto : BaseEntity<int>
    {
        private Producto() : base()
        {
            Codigo = string.Empty;
            Nombre = string.Empty;
            Genero = "unisex";
        }

        private Producto(string pCodigo, string pNombre, int pMarcaId, int pCategoriaId, string? pDescripcion,
            string pGenero, string? pTemporada, string? pMaterial, int? pPesoGramos, string? pTipoCasco,
            string? pHomologacion, bool? pHomologacionVigente, DateTime? pFechaVencHomologacion, bool pActivo)
        {
            Codigo = pCodigo;
            Nombre = pNombre;
            MarcaId = pMarcaId;
            CategoriaId = pCategoriaId;
            Descripcion = pDescripcion;
            Genero = pGenero;
            Temporada = pTemporada;
            Material = pMaterial;
            PesoGramos = pPesoGramos;
            TipoCasco = pTipoCasco;
            Homologacion = pHomologacion;
            HomologacionVigente = pHomologacionVigente;
            FechaVencHomologacion = pFechaVencHomologacion;
            Activo = pActivo;
        }

        public string Codigo { get; private set; }
        public string Nombre { get; private set; }
        public int MarcaId { get; private set; }
        public int CategoriaId { get; private set; }
        public string? Descripcion { get; private set; }
        public string Genero { get; private set; }
        public string? Temporada { get; private set; }
        public string? Material { get; private set; }
        public int? PesoGramos { get; private set; }
        public string? TipoCasco { get; private set; }
        public string? Homologacion { get; private set; }
        public bool? HomologacionVigente { get; private set; }
        public DateTime? FechaVencHomologacion { get; private set; }
        public bool Activo { get; private set; }

        public static Result<Producto> Crear(string codigo, string nombre, int marcaId, int categoriaId, string? descripcion,
            string genero, string? temporada, string? material, int? pesoGramos, string? tipoCasco,
            string? homologacion, bool? homologacionVigente, DateTime? fechaVencHomologacion, bool activo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return Result.Fail<Producto>("Codigo es requerido");
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Producto>("Nombre es requerido");
            if (marcaId <= 0) return Result.Fail<Producto>("Marca es requerida");
            if (categoriaId <= 0) return Result.Fail<Producto>("Categoria es requerida");
            return new Producto(codigo, nombre, marcaId, categoriaId, descripcion,
                string.IsNullOrWhiteSpace(genero) ? "unisex" : genero, temporada, material, pesoGramos, tipoCasco,
                homologacion, homologacionVigente, fechaVencHomologacion, activo);
        }

        public Result<Producto> Modificar(string pCodigo, string pNombre, int pMarcaId, int pCategoriaId, string? pDescripcion,
            string pGenero, string? pTemporada, string? pMaterial, int? pPesoGramos, string? pTipoCasco,
            string? pHomologacion, bool? pHomologacionVigente, DateTime? pFechaVencHomologacion, bool pActivo)
        {
            if (string.IsNullOrWhiteSpace(pCodigo)) return Result.Fail<Producto>("Codigo es requerido");
            if (string.IsNullOrWhiteSpace(pNombre)) return Result.Fail<Producto>("Nombre es requerido");
            if (pMarcaId <= 0) return Result.Fail<Producto>("Marca es requerida");
            if (pCategoriaId <= 0) return Result.Fail<Producto>("Categoria es requerida");
            Codigo = pCodigo;
            Nombre = pNombre;
            MarcaId = pMarcaId;
            CategoriaId = pCategoriaId;
            Descripcion = pDescripcion;
            Genero = string.IsNullOrWhiteSpace(pGenero) ? "unisex" : pGenero;
            Temporada = pTemporada;
            Material = pMaterial;
            PesoGramos = pPesoGramos;
            TipoCasco = pTipoCasco;
            Homologacion = pHomologacion;
            HomologacionVigente = pHomologacionVigente;
            FechaVencHomologacion = pFechaVencHomologacion;
            Activo = pActivo;
            return this;
        }
    }
}
