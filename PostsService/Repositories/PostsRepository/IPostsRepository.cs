using Microsoft.EntityFrameworkCore;
using PostsService.Entities.Posts;
using PostsService.Protos;
using System.Linq.Expressions;
namespace PostsService.Repositories.PostsRepository
{
    public interface IPostsRepository : IGenericPostRepository<Posts>
    {
        public Task<Posts> GetAsync(Guid id);
        public Task<(List<Posts> posts, uint pagesCount)> GetListAsync(uint page_num, uint page_size, bool isGettingPage, string substring);
        //public Task<int> CompleteAsync();
    }
}
