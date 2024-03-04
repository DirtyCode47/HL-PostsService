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

        public async Task<(List<Posts> posts, uint pagesCount)> GetListAsync(uint page_num, uint page_size, string substring)
        {
            IQueryable<Posts> postsQuery = _dbContext.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(substring))
            {
                string lower_substring = substring.ToLower();

                postsQuery = postsQuery.Where(u =>
                    EF.Functions.Like(u.Name.ToString().ToLower(), lower_substring) ||
                    EF.Functions.Like(u.Code.ToString().ToLower(), lower_substring) ||
                    EF.Functions.Like(u.River.ToString().ToLower(), lower_substring
                    ));
            }

            postsQuery.Cast<Posts>().OrderBy(p => p.Code);

            var postsList = await postsQuery.ToListAsync();

            uint pagesCount = (uint)(postsList.Count / page_size);
            if (postsList.Count % (int)page_size != 0)
                pagesCount++;

            int skipCount = (int)(page_num * page_size);
            int takeCount = (int)(page_num * page_size + page_size);
            var pagePosts = postsList.Skip(skipCount).Take(takeCount).ToList();

            return (pagePosts, pagesCount);
        }

    }
}
