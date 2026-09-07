using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Colores
{
    public interface IColorRepositorio : IRepository<Color, int>
    {
        public List<Color> GetColores(Spec<Color> specification, int skip, int take);
        public List<Color> GetColores(Spec<Color> specification);
        public Task<List<Color>> GetColoresAsync(Spec<Color> specification);
        public Task<List<Color>> GetColoresAsync(Spec<Color> specification, int skip, int take);
    }
}
