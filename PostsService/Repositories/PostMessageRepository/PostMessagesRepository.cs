using Microsoft.EntityFrameworkCore;
using PostsService.Entities.PostMessage;

namespace PostsService.Repositories.PostMessageRepository
{
    public class PostMessagesRepository : GenericPostRepository<PostMessage>, IPostMessageRepository
    {
        private PostsServiceDbContext _dbContext;
        public PostMessagesRepository(PostsServiceDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<PostMessage>> GetAll()
        {
            return await _dbContext.PostMessages.ToListAsync();
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
