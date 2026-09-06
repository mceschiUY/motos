using Microsoft.EntityFrameworkCore;
using ApiMotos.Domain.Agregates.Documentos;
using ApiMotos.Infrastructure.Common;

namespace ApiMotos.Infrastructure.Agregates.Documentos.Persistence
{
    public class DocumentoRepositorio : BaseRepository<Documento, int>, IDocumentoRepositorio
    {
        private readonly DocumentoContext _context;

        public DocumentoRepositorio(DocumentoContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Documento>> GetDocumentosByRelacionAsync(int relacionId, string relacionNombre)
        {
            return await _context.Documentos
                .AsNoTracking()
                .Where(d => d.RelacionId == relacionId && d.RelacionNombre == relacionNombre)
                .OrderByDescending(d => d.FechaCarga)
                .ToListAsync();
        }

        public async Task<Documento?> GetDocumentoConContenidoAsync(int id)
        {
            return await _context.Documentos
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
