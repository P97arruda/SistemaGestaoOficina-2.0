using SistemaGestaoOficina.Data.Entities;

namespace SistemaGestaoOficina.Data
{
    public interface IGenericRepository<T> where T : class
    {
        public IQueryable<T> GetAll();

        public Task<T> GetByIdAsync(int Id);

        public Task CreateAsync(T entity);

        public Task UpdateAsync(T entity);

        public Task DeleteAsync(T entity);

        public Task<bool> ExistAsync(int id);
    }
}
