using PostsService.Entities.PostMessage;
namespace PostsService.Repositories.PostMessageRepository
{
    public interface IPostMessageRepository : IGenericPostRepository<PostMessage>
    {
        public Task<List<PostMessage>> GetAll();
        public Task<int> CompleteAsync();
    }
}
