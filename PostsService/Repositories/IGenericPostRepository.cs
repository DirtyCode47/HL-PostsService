using Microsoft.EntityFrameworkCore;
using PostsService.Entities;

namespace PostsService.Repositories
{
    public interface IGenericPostRepository<TEntity> where TEntity : class, IPosts
    {
        public Task<TEntity> AddAsync(TEntity entity);
        public TEntity Delete(TEntity entity);
        public TEntity Update(TEntity entity);
        public IQueryable<TEntity> GetAll();
    }
}