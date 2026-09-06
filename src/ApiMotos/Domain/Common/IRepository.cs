using Microsoft.EntityFrameworkCore.Storage;
using ApiMotos.Infrastructure.Common;

namespace ApiMotos.Domain.Common
{
    public interface IRepository<T, Tid> where T : BaseEntity<Tid>
    {
        public Task<T> AddAsync(T entidad);
        void Update(T entidad);
        void Delete(T entidad);
        public T Get(Tid id);
        public T? Find(Tid id);
        public Task<T?> FindAsync(Tid id);
        public T Add(T entidad);

        public Task<List<T>>? GetAllAsync();

       


    }
}
