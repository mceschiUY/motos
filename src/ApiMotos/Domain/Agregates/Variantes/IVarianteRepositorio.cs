using ApiMotos.Domain.Common;
using NSpecifications;

namespace ApiMotos.Domain.Agregates.Variantes
{
    public interface IVarianteRepositorio : IRepository<Variante, int>
    {
        public List<Variante> GetVariantes(Spec<Variante> specification, int skip, int take);
        public List<Variante> GetVariantes(Spec<Variante> specification);
        public Task<List<Variante>> GetVariantesAsync(Spec<Variante> specification);
        public Task<List<Variante>> GetVariantesAsync(Spec<Variante> specification, int skip, int take);
    }
}
