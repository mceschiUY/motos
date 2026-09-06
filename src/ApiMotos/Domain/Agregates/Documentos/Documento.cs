using FluentResults;
using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Documentos
{
    public class Documento : BaseEntity<int>
    {
        private Documento() : base()
        {
            Nombre = string.Empty;
            Extension = string.Empty;
            Contenido = Array.Empty<byte>();
            MimeType = string.Empty;
            RelacionNombre = string.Empty;
        }

        private Documento(string nombre, string extension, byte[] contenido, string mimeType, int relacionId, string relacionNombre)
        {
            Nombre = nombre;
            Extension = extension;
            Contenido = contenido;
            MimeType = mimeType;
            FechaCarga = DateTime.Now;
            RelacionId = relacionId;
            RelacionNombre = relacionNombre;
        }

        public string Nombre { get; private set; }
        public string Extension { get; private set; }
        public byte[] Contenido { get; private set; }
        public string MimeType { get; private set; }
        public DateTime FechaCarga { get; private set; }
        public int RelacionId { get; private set; }
        public string RelacionNombre { get; private set; }

        public static Result<Documento> Crear(string nombre, string extension, byte[] contenido, string mimeType, int relacionId, string relacionNombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Fail<Documento>("El nombre del documento es requerido");

            if (contenido == null || contenido.Length == 0)
                return Result.Fail<Documento>("El contenido del documento es requerido");

            if (string.IsNullOrWhiteSpace(mimeType))
                return Result.Fail<Documento>("El tipo MIME es requerido");

            return new Documento(nombre, extension, contenido, mimeType, relacionId, relacionNombre);
        }
    }
}
