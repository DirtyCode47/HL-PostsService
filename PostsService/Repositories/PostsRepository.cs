using Microsoft.EntityFrameworkCore;
using PostsService.Entities;
using PostsService.Exceptions;
using PostsService.Protos;
using System;
using System.Linq.Expressions;
using static Grpc.Core.Metadata;

namespace PostsService.Repositories
{
    public class PostsRepository
    {
        private PostsServiceDbContext _dbContext;
        public PostsRepository(PostsServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Posts Add(Posts post)
        {
            return _dbContext.Posts.Add(post).Entity;
        }

        public async Task<Posts> AddAsync(Posts post)
        {
            var added_post_entry = await _dbContext.Posts.AddAsync(post);
            return added_post_entry.Entity;
        }

        public Posts Get(Guid id)
        {
            return _dbContext.Posts.Find(id);
        }

        public async Task<Posts> GetAsync(Guid id)
        {
            return await _dbContext.Posts.FindAsync(id);
        }

        public async Task<Posts> FindByCodeAsync(string code)
        {
            return await _dbContext?.Posts?.FirstOrDefaultAsync(p => p.Code == code);
        }

        public Posts Delete(Posts post)
        {
            return _dbContext.Posts.Remove(post).Entity;
        }


        
        public Posts Update(Posts post)
        {
            return _dbContext.Posts.Update(post).Entity;
        }

        public IEnumerable<Posts> Find(string substring)
        {
            throw new NotImplementedException();
        }


        //public IEnumerable<Posts> GetPage(int page, int page_size)
        //{
        //    List<Posts> allPosts = GetAllPosts().ToList();
        //    return allPosts.GetRange(((page - 1) * page_size), page_size).ToArray();
        //}


        public async Task<(IEnumerable<Posts> postPage,uint maxPage)> GetPageAsync(uint page_num, uint page_size)
        {
            var posts = await _dbContext.Posts.ToListAsync();

            uint maxPage = (uint)(posts.Count / 10) + 1; // Количество страниц

            posts.Sort((a, b) => a.Code.CompareTo(b.Code));
            var pagePosts = posts.Skip(((int)page_num - 1) * 10).Take(10).ToList();

            return (pagePosts,maxPage);
        }


        public async Task<List<Posts>> GetUnsentKafkaMessagesAsync()
        {
            return await _dbContext.Posts
                .Where(post => !post.IsKafkaMessageSended)
                .ToListAsync();
        }

        public IEnumerable<Posts> GetAllPosts()
        {
            return _dbContext.Posts;
        }

        public async Task<IEnumerable<Posts>> GetAllPostsAsync()
        {
            return await _dbContext.Posts.ToListAsync();
        }

        public bool IsAny(Expression<Func<Posts,bool>> predicate)
        {
            return _dbContext.Posts.Any(predicate);
        }

        public int GetMaxPage(int page_size)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsAsync(Guid postId)
        {
            return await _dbContext.Posts.AnyAsync(post => post.Id == postId);
        }

        //public IEnumerable<Posts> SearchWithSubstring(List<string> search_words)
        //{
        //    var all_posts = _dbContext.Posts;
        //    if (search_words.Count == 1)
        //    {
        //        return all_posts
        //               .AsEnumerable()
        //               .Where(post => SearchSubstringInDb(post, search_words[0]));
        //    }

        //    List<Posts> posts = all_posts.ToList();
        //    List<Posts> posts_to_delete = new();

        //    for (int i = 0; i < posts.Count; i++)
        //    {
        //        for (int j = 0; j < search_words.Count; j++)
        //        {
        //            if (!SearchSubstringInDb(posts[i], search_words[j]))
        //            { 
        //                posts_to_delete.Add(posts[i]);
        //                break;
        //            }
        //        }
        //    }

        //    foreach (var element in posts_to_delete)
        //    {
        //        posts.Remove(element);
        //    }

        //    return posts;
        //}
        //private bool ContainsSubstring(string original, string substring)
        //{
        //    return original.ToLower().Contains(substring.ToLower());
        //}
        //private bool SearchSubstringInDb(Posts post, string substring)
        //{
        //    return ContainsSubstring(post.Id.ToString().ToLower(), substring) ||
        //                    ContainsSubstring(post.Code.ToLower(), substring) ||
        //                    ContainsSubstring(post.Name.ToLower(), substring) ||
        //                    ContainsSubstring(post.River.ToLower(), substring);
        //}




        public void Complete()
        {
            _dbContext.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

   
    }
}
