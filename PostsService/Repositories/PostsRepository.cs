using Microsoft.EntityFrameworkCore;
using PostsService.Entities;
using PostsService.Exceptions;
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
            if (post == null) throw new NoSuchRecordInDbException("Can't find record in Db with this id");
            return post;
        }
        public void Complete()
        {
            _dbContext.SaveChanges();
        }
    }
}
