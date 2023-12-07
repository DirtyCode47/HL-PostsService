using PostsService.Entities;
namespace PostsService.Repositories
{
    public interface IPostsReepository
    {
        public Posts Add(Posts post);
        public Posts Get(int PostCode);
        public Posts Delete(Posts post);
        public Posts Update(Posts post);
        public IEnumerable<Posts> Find(string substring);
        public IEnumerable<Posts> GetPage(Guid id);
        public int GetMaxPage(int page_size);
    }
}
