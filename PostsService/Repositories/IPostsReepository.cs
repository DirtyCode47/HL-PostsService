using Microsoft.EntityFrameworkCore;
using PostsService.Entities;
using System.Linq.Expressions;
namespace PostsService.Repositories
{
    public interface IPostsReepository
    {
        public Posts Add(Posts post);

        public Task<Posts> AddAsync(Posts post);

        public Posts Get(Guid id);

        public Task<Posts> GetAsync(Guid id);

        public Task<Posts> FindByCodeAsync(string code);

        public Posts Delete(Posts post);

        public Posts Update(Posts post);

        public Task<(List<Posts> postPage, uint maxPage)> GetPageAsync(uint page_num, uint page_size);

        //public Task<List<Posts>> FindUnloadedPostsAsync();

        public bool IsAny(Expression<Func<Posts, bool>> predicate);

        public Task<bool> ExistsAsync(Guid postId);

        public Task<List<Posts>> FindWithSubstring(string substring);

        public void Complete();

        public Task<int> CompleteAsync();
    }
}
