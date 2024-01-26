using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PostsService.Cache;
using PostsService.Entities;
using PostsService.Protos;
using PostsService.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostsService.Services
{
    public class PostsServiceImpl : Protos.PostsService.PostsServiceBase
    {
        private readonly PostsRepository _postsRepository;
        private readonly CacheService _cacheService;

        public PostsServiceImpl(PostsRepository postsRepository, CacheService cacheService)
        {
            _postsRepository = postsRepository ?? throw new ArgumentNullException(nameof(postsRepository));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            //Guid postId = Guid.Parse(request.Post.Id);

            Guid postId = Guid.NewGuid();
            Posts post = new Posts() { Id = postId, Code = request.Post.Code, Name = request.Post.Name, River = request.Post.River };


            
            if (await _postsRepository.GetAsync(postId) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Record with this id already exists in the database"));
            }

            if (await _postsRepository.FindByCode(request.Post.Code) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Record with this post code already exists in the database"));
            }

            Posts addedPost = await _postsRepository.AddAsync(post);
            await _postsRepository.CompleteAsync();

            // Обновляем кэш
            _cacheService.AddOrUpdateCache($"post:{addedPost.Id}", addedPost);

            return new CreateResponse { Post = request.Post };
        }

        public override async Task<DeleteResponse> Delete(DeleteRequest request, ServerCallContext context)
        {
            if(!Guid.TryParse(request.Id,out Guid postId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Uncorrect guid format"));
            }
            
            Posts entity = await _postsRepository.GetAsync(postId);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find a record in the database with this id"));
            }

            _postsRepository.Delete(entity);
            await _postsRepository.CompleteAsync();

            // Удаляем из кэша
            _cacheService.ClearCache($"post:{entity.Id}");

            return new DeleteResponse
            {
                Post = new Post
                {
                    Id = entity.Id.ToString(),
                    Code = entity.Code,
                    Name = entity.Name,
                    River = entity.River,
                }
            };
        }

        public override async Task<UpdateResponse> Update(UpdateRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Post.Id, out Guid postId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Uncorrect guid format"));
            }

            // Найти существующую сущность по Id
            var existingPost = _postsRepository.Get(postId);

            if (existingPost == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find a record in the database with this id"));
            }

            if(await _postsRepository.FindByCode(request.Post.Code) != null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Post with such code already exists in DB"));
            }

            // Обновить свойства существующей сущности
            existingPost.Code = request.Post.Code;
            existingPost.Name = request.Post.Name;
            existingPost.River = request.Post.River;

            // Вызвать метод Update для сохранения изменений
            _postsRepository.Update(existingPost);
            await _postsRepository.CompleteAsync();

            // Обновить кэш
            _cacheService.AddOrUpdateCache($"post:{existingPost.Id}", existingPost);

            // Вернуть обновленную сущность
            return new UpdateResponse
            {
                Post = new Post
                {
                    Id = existingPost.Id.ToString(),
                    Code = existingPost.Code,
                    Name = existingPost.Name,
                    River = existingPost.River,
                }
            };
        }

        public override async Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out Guid guid))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Uncorrect guid format"));
            }

            Posts post = _cacheService.GetFromCache<Posts>($"post:{guid}");

            if (post == null)
            {
                // Если записи нет в кэше, пытаемся получить из базы данных
                post = await _postsRepository.GetAsync(guid);
                if (post == null)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find a record in the database with this id"));
                }

                // Если нашли в базе, добавляем в кэш
                _cacheService.AddOrUpdateCache($"post:{post.Id}", post);
            }

            return new GetResponse
            {
                Post = new Post()
                {
                    Id = post.Id.ToString(),
                    Code = post.Code,
                    Name = post.Name,
                    River = post.River
                }
            };
        }

        public override Task<GetPageResponse> GetPage(GetPageRequest request, ServerCallContext context)
        {
            uint maxPage = (uint)(_postsRepository.GetAllPosts().Count() / 10) + 1; // Количество страниц
            var posts = _cacheService.GetAllFromCache<Posts>("post:*");

            if (posts == null)
            {
                // Если записей нет в кэше, получаем из базы и добавляем в кэш
                posts = _postsRepository.GetAllPosts().ToList();
                foreach (var post in posts)
                {
                    _cacheService.AddOrUpdateCache($"post:{post.Id}", post);
                }
            }

            posts.Sort((a, b) => a.Code.CompareTo(b.Code));
            var pagedPosts = posts.Skip(((int)request.PageNumber - 1) * 10).Take(10).ToList();

            if (!pagedPosts.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find elements on this page"));
            }

            GetPageResponse getPageResponse = new GetPageResponse();

            foreach (var post in pagedPosts)
            {
                getPageResponse.Posts.Add(new Post
                {
                    Id = post.Id.ToString(),
                    Name = post.Name,
                    Code = post.Code,
                    River = post.River
                });
            }

            getPageResponse.PageNumber = request.PageNumber;
            getPageResponse.MaxPageNumber = maxPage;

            return Task.FromResult(getPageResponse);
        }

        public override Task<FindResponse> Find(FindRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Substring))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request has an empty string"));
            }

            string lower_substring = request.Substring.ToLower();
            List<string> words = lower_substring.Split(' ').ToList();

            List<Posts> posts = _cacheService.GetAllFromCache<Posts>("post:*");


            if (posts == null)
            {
                // Если записей нет в кэше, получаем из базы и добавляем в кэш
                posts = _postsRepository.GetAllPosts().ToList();
                foreach (var post in posts) 
                {
                    _cacheService.AddOrUpdateCache($"post:{post.Id}", post);
                }
            }

            posts.Sort((a,b) => a.Code.CompareTo(b.Code));
            var responsePosts = new List<Post>();

            foreach (var post in posts)
            {
                if (words.All(word =>
                    post.Id.ToString().ToLower().Contains(word) ||
                    post.Name.ToLower().Contains(word) ||
                    post.Code.ToLower().Contains(word) ||
                    post.River.ToLower().Contains(word)))
                {
                    responsePosts.Add(new Post
                    {
                        Id = post.Id.ToString(),
                        Name = post.Name,
                        Code = post.Code,
                        River = post.River
                    });
                }
            }

            if (!responsePosts.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Haven't found a record in the database with this substring"));
            }

            FindResponse response = new FindResponse();
            response.Posts.AddRange(responsePosts);

            return Task.FromResult(response);
        }

        public override Task<GetAllResponse> GetAll(GetAllRequest request, ServerCallContext context)
        {
            var posts = _cacheService.GetAllFromCache<Posts>("post:*");

            if (posts == null)
            {
                posts = _postsRepository.GetAllPosts().ToList(); //Надо не забыть добавить в кэш
            }  
            

            var response = new GetAllResponse();

            foreach (var post in posts)
            {
                response.Posts.Add(new Post
                {
                    Id = post.Id.ToString(),
                    Name = post.Name,
                    Code = post.Code,
                    River = post.River
                });
            }

            return Task.FromResult(response);
        }
    }
}
