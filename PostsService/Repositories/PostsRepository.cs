using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PostsService.Entities;
using PostsService.Protos;
using System;
using System.Linq.Expressions;
using static Grpc.Core.Metadata;

namespace PostsService.Repositories
{
    public class PostsRepository:GenericPostRepository<Posts>,IPostsRepository
    {
        private PostsServiceDbContext _dbContext;
        public PostsRepository(PostsServiceDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Posts> GetAsync(Guid id)
        {
            return await _dbContext.Posts.FindAsync(id);
        }

        public async Task<Posts> FindByCodeAsync(string code)
        {
            return await _dbContext?.Posts?.FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<(List<Posts> postPage,uint maxPage)> GetPageAsync(uint page_num, uint page_size)
        {
            Index start = new((int)page_num * 10, false);
            Index end = new((int)(page_num * 10 + page_size), false);
            Range range = new Range(start, end);

            var posts = GetAll();
            posts.Cast<Posts>().OrderBy(p => p.Code).Take(range);

            var postsList = await posts.ToListAsync();
            uint maxPage = (uint)(posts.Count() / 10) + 1; // Количество страниц
            return (postsList, maxPage);
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
    }
}
