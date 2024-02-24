using Microsoft.EntityFrameworkCore;
using PostsService.Entities;
using PostsService.Protos;
using System.Linq.Expressions;
namespace PostsService.Repositories
{
    public interface IPostsReepository:IGenericPostRepository<Posts>
    {
        public Task<Posts> GetAsync(Guid id);
        public Task<Posts> FindByCodeAsync(string code);
        public Task<(List<Posts> postPage, uint maxPage)> GetPageAsync(uint page_num, uint page_size);
        public Task<List<Posts>> FindWithSubstring(string substring);
        //public Task<int> CompleteAsync();
    }
}
