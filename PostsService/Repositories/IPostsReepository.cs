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
        public IEnumerable<Posts> GetPage(int page, int page_size);
        public int GetMaxPage(int page_size);
    }
}
