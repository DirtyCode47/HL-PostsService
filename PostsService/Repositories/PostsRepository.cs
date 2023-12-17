using Microsoft.EntityFrameworkCore;
using PostsService.Entities;
using PostsService.Exceptions;
using System.Linq.Expressions;
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
            //if(post.River.Length>255 || post.Name.Length>255 || post.Code.Length > 5)
            //{
            //    throw new IncorrectLengthException("123");
            //}
            if(_dbContext.Posts.Any(a => a.River == post.River && a.Name == post.Name && a.Code == post.Code))
            {
                throw new ExistRecordInDbException("This record already exist in Db");
            }
            return _dbContext.Posts.Add(post).Entity;
        }
        public Posts Get(Guid id)
        {
            Posts? post = _dbContext.Posts.Find(id);
            if (post == null) throw new NoSuchRecordInDbException("Can't find record in Db with this id");
            return post;
        }
        //public bool Any(Expression<Func<Posts,bool>> predicate)
        //{
        //    return _dbContext.Set<Posts>().Any(predicate);
        //}
        public void Complete()
        {
            _dbContext.SaveChanges();
        }
    }
}
