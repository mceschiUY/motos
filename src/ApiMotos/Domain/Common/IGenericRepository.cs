using NSpecifications;

namespace ApiMotos.Domain.Common
{
    public interface IGenericRepository<T> : IRepository<T, int> where T : BaseEntity<int>
    {
        List<T> GetBySpec(Spec<T> specification);
        List<T> GetBySpec(Spec<T> specification, int skip, int take);
        Task<List<T>> GetBySpecAsync(Spec<T> specification);
        Task<List<T>> GetBySpecAsync(Spec<T> specification, int skip, int take);
    }
}
