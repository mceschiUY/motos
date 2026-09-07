using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Categorias
{
    public interface ICategoriaRepositorio : IRepository<Categoria, int>
    {
        public List<Categoria> GetCategorias(Spec<Categoria> specification, int skip, int take);
        public List<Categoria> GetCategorias(Spec<Categoria> specification);
        public Task<List<Categoria>> GetCategoriasAsync(Spec<Categoria> specification);
        public Task<List<Categoria>> GetCategoriasAsync(Spec<Categoria> specification, int skip, int take);
    }
}
