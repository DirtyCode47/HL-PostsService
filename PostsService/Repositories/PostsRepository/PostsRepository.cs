﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PostsService.Entities.Posts;
using PostsService.Protos;
using System;
using System.Linq.Expressions;
using static Grpc.Core.Metadata;

namespace PostsService.Repositories.PostsRepository
{
    public class PostsRepository : GenericPostRepository<Posts>, IPostsRepository
    {
        private PostsServiceDbContext _dbContext;
        public PostsRepository(PostsServiceDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Posts> GetAsync(Guid id)
        {
            return await _dbContext.Posts.FindAsync(id);
        }

        public async Task<(List<Posts> posts, uint pagesCount)> GetManyAsync(uint page_num, uint page_size, string substring)
        {
            IQueryable<Posts> postsQuery = _dbContext.Posts.AsQueryable();

            if (page_size >= 0 && page_size > 0)
            {
                if (page_size > 100) page_size = 100; //ограничение кол-ва постов на странице (макс - 100)

                Index start = new((int)(page_num * page_size), false);
                Index end = new((int)(page_num * page_size + page_size), false);
                Range range = new Range(start, end);

                postsQuery.Cast<Posts>().OrderBy(p => p.Code).Take(range);
            }

            if (!string.IsNullOrEmpty(substring)) 
            {
                string lower_substring = substring.ToLower();
                postsQuery = postsQuery.Where(u => 
                    EF.Functions.Like(u.Id.ToString().ToLower(), lower_substring) ||
                    EF.Functions.Like(u.Name.ToString().ToLower(), lower_substring) ||
                    EF.Functions.Like(u.Code.ToString().ToLower(), lower_substring) ||
                    EF.Functions.Like(u.River.ToString().ToLower(), lower_substring
                    ));
            }

            var postsList = await postsQuery.ToListAsync();

            uint pages_count = (uint)(postsQuery.Count() / page_size);
            if (postsQuery.Count() % (int)page_size != 0)
                pages_count++;

            return (postsList, pages_count);
        }

    }
}
