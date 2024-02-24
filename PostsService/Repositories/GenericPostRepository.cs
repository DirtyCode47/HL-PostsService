using Microsoft.EntityFrameworkCore;
using PostsService.Entities;

namespace PostsService.Repositories
{
    public class GenericPostRepository<TEntity> : IGenericPostRepository<TEntity> where TEntity : class, IPosts
    {
        private PostsServiceDbContext _dbContext;
        public GenericPostRepository(PostsServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            var addedPost = await _dbContext.Set<TEntity>().AddAsync(entity);
            return addedPost.Entity;
        }
        public async Task<TEntity> Delete(TEntity entity)
        {
            return _dbContext.Set<TEntity>().Remove(entity).Entity;
        }
        public async Task<TEntity> Update(TEntity entity)
        {
            return _dbContext.Set<TEntity>().Update(entity).Entity;
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbContext.Set<TEntity>().ToListAsync();
        }
    }
}
