using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PostsService.Entities.Posts;
using PostsService.Protos;
using System;
using System.Linq.Expressions;
using static Grpc.Core.Metadata;

namespace PostsService.Repositories.PostsRepository
{
    public class PostsRepository : GenericPostRepository<Posts>, IPostsRepository
    {
        private PostsServiceDbContext _dbContext;
        public PostsRepository(PostsServiceDbContext dbContext) : base(dbContext)
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

        public async Task<(List<Posts> postPage, uint maxPage)> GetPageAsync(uint page_num, uint page_size)
        {
            Index start = new((int) (page_num * page_size), false);
            Index end = new((int)(page_num * page_size + page_size), false);
            Range range = new Range(start, end);

            var posts = _dbContext.Posts.AsQueryable();
            posts.Cast<Posts>().OrderBy(p => p.Code).Take(range);

            var postsList = await posts.ToListAsync();

            uint pagesCount = (uint)(posts.Count() / page_size);
            if (posts.Count() % (int)page_size != 0)
                pagesCount++;

            return (postsList, pagesCount);
        }

        public async Task<List<Posts>> FindWithSubstring(string substring)
        {
            string lower_substring = substring.ToLower();

            IQueryable<Posts> postsQuery = _dbContext.Posts.AsQueryable();
            postsQuery = postsQuery.Where(u => EF.Functions.Like(u.Id.ToString().ToLower(), lower_substring));
            postsQuery = postsQuery.Where(u => EF.Functions.Like(u.Name.ToString().ToLower(), lower_substring));
            postsQuery = postsQuery.Where(u => EF.Functions.Like(u.Code.ToString().ToLower(), lower_substring));
            postsQuery = postsQuery.Where(u => EF.Functions.Like(u.River.ToString().ToLower(), lower_substring));

            return await postsQuery.ToListAsync();
        }
    }
}
