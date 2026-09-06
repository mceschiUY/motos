using ApiMotos.Domain.Common;

namespace ApiMotos.Domain.Agregates.Documentos
{
    public interface IDocumentoRepositorio : IRepository<Documento, int>
    {
        Task<List<Documento>> GetDocumentosByRelacionAsync(int relacionId, string relacionNombre);
        Task<Documento?> GetDocumentoConContenidoAsync(int id);
    }
}
