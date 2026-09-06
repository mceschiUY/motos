namespace ApiMotos.Application.Agregates.Documentos.Queries.Documentos
{
    public class DocumentosDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public DateTime FechaCarga { get; set; }
        public int RelacionId { get; set; }
        public string RelacionNombre { get; set; } = string.Empty;
        // Nota: Contenido NO se incluye para optimizar listas
    }

    public class DocumentoContenidoDto : DocumentosDto
    {
        public byte[] Contenido { get; set; } = Array.Empty<byte>();
    }
}
