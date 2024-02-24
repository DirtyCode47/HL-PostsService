using PostsService.Entities;

namespace PostsService.Repositories
{
    public class PostMessagesRepository
    {
        private PostsServiceDbContext _dbContext;
        public PostMessagesRepository(PostsServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PostMessage> AddPostMessageAsync(PostMessage postMessage)
        {
            var addedPost = await _dbContext.PostMessages.AddAsync(postMessage);
            return addedPost.Entity;
        }
    }
}
