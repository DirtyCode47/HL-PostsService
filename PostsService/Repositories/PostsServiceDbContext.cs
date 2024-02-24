using Microsoft.EntityFrameworkCore;
using PostsService.Entities.PostMessage;
using PostsService.Entities.Posts;

namespace PostsService.Repositories
{
    public class PostsServiceDbContext:DbContext
    {
        public PostsServiceDbContext(DbContextOptions<PostsServiceDbContext> options) : base(options) { }

        public DbSet<Posts> Posts { get; set; }
        public DbSet<PostMessage> PostMessages { get; set; }
    }
}
