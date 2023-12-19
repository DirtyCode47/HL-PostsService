using Microsoft.EntityFrameworkCore;
using PostsService.Entities;
using PostsService.Exceptions;
using System.Linq.Expressions;
using static Grpc.Core.Metadata;
namespace PostsService.Repositories
{
    public class PostsRepository //:IPostsReepository
    {
        private PostsServiceDbContext _dbContext;
        public PostsRepository(PostsServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Posts Add(Posts post)
        {
            return _dbContext.Posts.Add(post).Entity;
        }

        public Posts Delete(Posts post)
        {
            return _dbContext.Posts.Remove(post).Entity;
        }

        public Posts Get(Guid id)
        {
            return _dbContext.Posts.Find(id);
        }

        public bool IsAny(Expression<Func<Posts,bool>> predicate)
        {
            return _dbContext.Posts.Any(predicate);
        }

        public void Complete()
        {
            _dbContext.SaveChanges();
        }
    }
}
