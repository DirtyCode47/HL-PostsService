using Microsoft.EntityFrameworkCore;
using PostsService.Entities;
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
        public Posts Get(Guid id)
        {
            Posts? post = _dbContext.Posts.Find(id);
            return _dbContext.Posts.Find(id);
        }
    }
}
