using Microsoft.EntityFrameworkCore;
using PostsService.Entities;

namespace PostsService.Repositories
{
    public class MessageRetryPostsRepository
    {
        private PostsServiceDbContext _dbContext;
        public MessageRetryPostsRepository(PostsServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public MessageRetryPosts Add(MessageRetryPosts post)
        {
            return _dbContext.messageRetryPosts.Add(post).Entity;
        }

        public async Task<MessageRetryPosts> AddAsync(MessageRetryPosts post)
        {
            var added_post_entry = await _dbContext.messageRetryPosts.AddAsync(post);
            return added_post_entry.Entity;
        }

        public MessageRetryPosts Delete(MessageRetryPosts post)
        {
            return _dbContext.messageRetryPosts.Remove(post).Entity;
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<MessageRetryPosts>> GetAllMessageRetryPostsAsync()
        {
            return await _dbContext.messageRetryPosts.ToListAsync();
        }
    }
}
