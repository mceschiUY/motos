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

        // Etapa C (plan §3.7) — catálogo premium. Todas nullable: en Development las agrega
        // DbBootstrap (ALTER ADD ... NULL) y los payloads viejos siguen valiendo.
        /// <summary>Aparece primero en el catálogo comercial.</summary>
        public bool? Destacado { get; private set; }
        /// <summary>Etiqueta "Nuevo" en la card del catálogo.</summary>
        public bool? Novedad { get; private set; }
        /// <summary>Markdown simple: peso, materiales, certificaciones, talle recomendado.</summary>
        public string? FichaTecnica { get; private set; }
        /// <summary>Id en PC_DOCUMENTOS de la foto de portada (se elige desde la galería de la ficha).</summary>
        public int? ImagenPrincipalId { get; private set; }

        public static Result<Producto> Crear(string codigo, string nombre, int marcaId, int categoriaId, string? descripcion,
            string genero, string? temporada, string? material, int? pesoGramos, string? tipoCasco,
            string? homologacion, bool? homologacionVigente, DateTime? fechaVencHomologacion, bool activo,
            bool? destacado = null, bool? novedad = null, string? fichaTecnica = null, int? imagenPrincipalId = null)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return Result.Fail<Producto>("Codigo es requerido");
            if (string.IsNullOrWhiteSpace(nombre)) return Result.Fail<Producto>("Nombre es requerido");
            if (marcaId <= 0) return Result.Fail<Producto>("Marca es requerida");
            if (categoriaId <= 0) return Result.Fail<Producto>("Categoria es requerida");
            var producto = new Producto(codigo, nombre, marcaId, categoriaId, descripcion,
                string.IsNullOrWhiteSpace(genero) ? "unisex" : genero, temporada, material, pesoGramos, tipoCasco,
                homologacion, homologacionVigente, fechaVencHomologacion, activo);
            producto.AsignarComercial(destacado, novedad, fichaTecnica, imagenPrincipalId);
            return producto;
        }

        public Result<Producto> Modificar(string pCodigo, string pNombre, int pMarcaId, int pCategoriaId, string? pDescripcion,
            string pGenero, string? pTemporada, string? pMaterial, int? pPesoGramos, string? pTipoCasco,
            string? pHomologacion, bool? pHomologacionVigente, DateTime? pFechaVencHomologacion, bool pActivo,
            bool? pDestacado = null, bool? pNovedad = null, string? pFichaTecnica = null, int? pImagenPrincipalId = null)
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
            AsignarComercial(pDestacado, pNovedad, pFichaTecnica, pImagenPrincipalId);
            return this;
        }

        /// <summary>Los 4 campos del catálogo premium, centralizados para que Crear y Modificar no se separen.</summary>
        private void AsignarComercial(bool? destacado, bool? novedad, string? fichaTecnica, int? imagenPrincipalId)
        {
            Destacado = destacado;
            Novedad = novedad;
            FichaTecnica = Limpiar(fichaTecnica);
            ImagenPrincipalId = imagenPrincipalId is > 0 ? imagenPrincipalId : null;
        }

        private static string? Limpiar(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }
}
