using Microsoft.EntityFrameworkCore;
using NSpecifications;
using ApiMotos.Domain.Common;

namespace ApiMotos.Infrastructure.Common
{
    public class GenericRepository<T> : BaseRepository<T, int>, IGenericRepository<T> where T : BaseEntity<int>
    {
        public GenericRepository(DbContext pContexto) : base(pContexto)
        {
        }

        public List<T> GetBySpec(Spec<T> specification)
        {
            return dbSet.Where(specification).ToList();
        }

        public List<T> GetBySpec(Spec<T> specification, int skip, int take)
        {
            return dbSet.Where(specification).Skip(skip).Take(take).ToList();
        }

        public async Task<List<T>> GetBySpecAsync(Spec<T> specification)
        {
            return await dbSet.AsNoTracking().Where(specification).ToListAsync();
        }

        public async Task<List<T>> GetBySpecAsync(Spec<T> specification, int skip, int take)
        {
            return await dbSet.AsNoTracking().Where(specification).Skip(skip).Take(take).ToListAsync();
        }
    }
}
