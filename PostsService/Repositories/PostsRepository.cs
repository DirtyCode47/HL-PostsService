using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PostsService.Entities;
using PostsService.Protos;
using System;
using System.Linq.Expressions;
using static Grpc.Core.Metadata;

namespace PostsService.Repositories
{
    public class PostsRepository:IPostsReepository
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

        public async Task<Posts> AddAsync(Posts post)
        {
            var added_post_entry = await _dbContext.Posts.AddAsync(post);
            return added_post_entry.Entity;
        }

        public Posts Get(Guid id)
        {
            return _dbContext.Posts.Find(id);
        }

        public async Task<Posts> GetAsync(Guid id)
        {
            return await _dbContext.Posts.FindAsync(id);
        }

        public IQueryable<Posts> GetAll()
        {
            return _dbContext.Posts.AsQueryable();
        }

        public async Task<Posts> FindByCodeAsync(string code)
        {
            return await _dbContext?.Posts?.FirstOrDefaultAsync(p => p.Code == code);
        }

        public Posts Delete(Posts post)
        {
            return _dbContext.Posts.Remove(post).Entity;
        }
        
        public Posts Update(Posts post)
        {
            return _dbContext.Posts.Update(post).Entity;
        }

        public async Task<(List<Posts> postPage,uint maxPage)> GetPageAsync(uint page_num, uint page_size)
        {
            Index start = new((int)page_num * 10,false);
            Index end = new((int)(page_num * 10 + page_size),false);
            Range range = new Range(start, end);

            var posts = GetAll();
            posts.Cast<Posts>().OrderBy(p => p.Code).Take(range);
            
            var postsList = await posts.ToListAsync();
            uint maxPage = (uint)(posts.Count() / 10) + 1; // Количество страниц
            return (postsList ,maxPage);
        }


        //public async Task<List<Posts>> FindUnloadedPostsAsync()
        //{
        //    return await GetAll()
        //        .Where(post => !post.IsKafkaMessageSended)
        //        .ToListAsync();
        //}

        public bool IsAny(Expression<Func<Posts,bool>> predicate)
        {
            return GetAll().Any(predicate);
        }

        public async Task<bool> ExistsAsync(Guid postId)
        {
            return await GetAll().AnyAsync(post => post.Id == postId);
        }

        public async Task<List<Posts>> FindWithSubstring(string substring)
        {
            string lower_substring = substring.ToLower();

            IQueryable<Posts> postsQuery = GetAll();
            postsQuery = postsQuery.Where(u => EF.Functions.Like(u.Id.ToString().ToLower(), lower_substring));
            postsQuery = postsQuery.Where(u => EF.Functions.Like(u.Name.ToString().ToLower(), lower_substring));
            postsQuery = postsQuery.Where(u => EF.Functions.Like(u.Code.ToString().ToLower(), lower_substring));
            postsQuery = postsQuery.Where(u => EF.Functions.Like(u.River.ToString().ToLower(), lower_substring));

            return await postsQuery.ToListAsync();
        }

        public void Complete()
        {
            _dbContext.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

    }
}
