using PostsService.Entities;

namespace PostsService.Repositories
{
    public class PostMessagesRepository:GenericPostRepository<PostMessage>,IPostMessageRepository
    {
        private PostsServiceDbContext _dbContext;
        public PostMessagesRepository(PostsServiceDbContext dbContext):base(dbContext)
        {
            _dbContext = dbContext;
        }
        
    }
}
