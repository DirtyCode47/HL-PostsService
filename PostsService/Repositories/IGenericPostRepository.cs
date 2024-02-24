using PostsService.Entities;

namespace PostsService.Repositories
{
    public interface IGenericPostRepository<TEntity> where TEntity : class, IPosts
    {
        public Task<TEntity> AddAsync(TEntity entity);
        public Task<TEntity> Delete(TEntity entity);
        public Task<IEnumerable<TEntity>> GetAllAsync();
        public Task<TEntity> Update(TEntity entity);
    }
}