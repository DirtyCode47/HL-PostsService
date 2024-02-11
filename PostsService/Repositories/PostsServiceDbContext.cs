using Microsoft.EntityFrameworkCore;
using PostsService.Entities;

namespace PostsService.Repositories
{
    public class PostsServiceDbContext:DbContext
    {
        public PostsServiceDbContext(DbContextOptions<PostsServiceDbContext> options) : base(options) { }

        public DbSet<Posts> Posts { get; set; }
        //public DbSet<MessageRetryPosts> messageRetryPosts { get; set; }
    }
}
