using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ApiMotos.Domain.Common;

namespace ApiMotos.Infrastructure.Common
{
    
    public class BaseRepository<T, Tid> : IRepository<T, Tid> where T : BaseEntity<Tid>
    {
        protected readonly DbContext _contexto;
        protected readonly DbSet<T> dbSet;

        public BaseRepository(DbContext pContexto)
        {
            _contexto = pContexto;
            dbSet = _contexto.Set<T>();
        }

        public T Add(T entidad)
        {
            var resul = dbSet.Add(entidad);
            _contexto.SaveChanges();
            return resul.Entity;
        }

        public async Task<T> AddAsync(T entidad)
        {

            using (var transaction = await _contexto.Database.BeginTransactionAsync())
            {
                try
                {
                    // Habilitar IDENTITY_INSERT
                   //await _contexto.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.RT_Simulaciones ON");

                    // Insertar la entidad
                    var result = await dbSet.AddAsync(entidad);

                    // Guardar los cambios
                    await _contexto.SaveChangesAsync();

                    // Deshabilitar IDENTITY_INSERT
                    //await _contexto.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.RT_Simulaciones OFF");

                    // Confirmar la transacci�n
                    await transaction.CommitAsync();

                    // Retornar la entidad
                    return result.Entity;
                }
                catch (Exception ex)
                {
                    // En caso de error, hacer rollback
                    await transaction.RollbackAsync();
                    throw;  // Volver a lanzar la excepci�n
                }
            }

        }

        public void Delete(T entidad)
        {
            dbSet.Remove(entidad);
            _contexto.SaveChanges();
        }

        public async Task<T?> FindAsync(Tid id)
        {

            return await dbSet.SingleOrDefaultAsync(x => x.Id.Equals(id));

        }

        public void Update(T entidad)
        {
            dbSet.Update(entidad);
            _contexto.SaveChanges();
        }

        public T Get(Tid id)
        {

            return dbSet.Single(x => x.Id.Equals(id));

        }

        public T? Find(Tid id)
        {

            var resul = dbSet.SingleOrDefault(x => x.Id.Equals(id));

            return resul;
        }

        public async Task<List<T>>? GetAllAsync()
        {

            var resul = dbSet.ToList();

            return resul;
        }


       



    }


}
