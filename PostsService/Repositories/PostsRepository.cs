using Microsoft.EntityFrameworkCore;
using PostsService.Entities;
using PostsService.Exceptions;
using System;
using System.Linq.Expressions;
using static Grpc.Core.Metadata;

namespace PostsService.Repositories
{
    public class PostsRepository : IPostsReepository
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
            return _dbContext.Posts.Find(id);
        }

        public Posts Delete(Posts post)
        {
            return _dbContext.Posts.Remove(post).Entity;
        }

        //НОВОЕ
        public Posts Update(Posts post)
        {
            return _dbContext.Posts.Update(post).Entity;
        }

        public IEnumerable<Posts> Find(string substring)
        {
            throw new NotImplementedException();
        }

        //НОВОЕ
        public IEnumerable<Posts> GetPage(int page, int page_size)
        {
            List<Posts> allPosts = GetAllPosts().ToList();
            return allPosts.GetRange(((page - 1) * page_size), page_size).ToArray();
        }

        //НОВОЕ
        public IEnumerable<Posts> GetAllPosts()
        {
            return _dbContext.Posts;
        }

        public bool IsAny(Expression<Func<Posts,bool>> predicate)
        {
            return _dbContext.Posts.Any(predicate);
        }

        public int GetMaxPage(int page_size)
        {
            throw new NotImplementedException();
        }



        public void Complete()
        {
            _dbContext.SaveChanges();
        }

   
    }
}
