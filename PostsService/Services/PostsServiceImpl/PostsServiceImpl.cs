using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PostsService.Protos;
using PostsService.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
//using Newtonsoft.Json;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;
using PostsService.Kafka;
//using static Google.Protobuf.Collections.MapField<TKey, TValue>;
using static Grpc.Core.Metadata;
using System.Xml.Linq;
using PostsService.Entities.Posts;
using PostsService.Entities.PostMessage;
using PostsService.Repositories.PostMessageRepository;
using PostsService.Repositories.PostsRepository;

namespace PostsService.Services.PostsServiceImpl
{
    public class PostsServiceImpl : Protos.PostsService.PostsServiceBase
    {
        private readonly PostsServiceDbContext _dbContext;
        private readonly IPostsRepository _postsRepository;
        private readonly IPostMessageRepository _postMessageRepository;
        public PostsServiceImpl(IPostsRepository postsRepository, IKafkaProducer kafkaProducer, PostsServiceDbContext dbContext, IPostMessageRepository postMessageRepository)
        {
            _dbContext = dbContext;
            _postsRepository = postsRepository;
            _postMessageRepository = postMessageRepository;
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            Guid postId = Guid.NewGuid();
            Posts post = new Posts() { Id = postId, Code = request.Code, Name = request.Name, River = request.River };

            if (await _postsRepository.GetAsync(postId) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Пост с таким ID уже существует в базе данных"));
            }

            Posts addedPost = await _postsRepository.AddAsync(post);

            PostMessage postMessage = new PostMessage() { Id = addedPost.Id, Code = addedPost.Code, Name = addedPost.Name, River = addedPost.River, postStatus = PostStatus.Added };
            await _postMessageRepository.AddAsync(postMessage);

            await _dbContext.SaveChangesAsync();

            return new CreateResponse { Post = new Post { Id = post.Id.ToString(), Code = post.Code, Name = post.Name, River = post.River } };
        }

        public override async Task<DeleteResponse> Delete(DeleteRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.Id, out Guid postId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Некорректный формат GUID"));
            }

            Posts entity = await _postsRepository.GetAsync(postId);

            if (entity == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Пост с таким ID не найден"));
            }

            _postsRepository.Delete(entity);

            var postMessage = new PostMessage() { Id = entity.Id, Code = entity.Code, Name = entity.Name, River = entity.River, postStatus = PostStatus.Deleted };
            await _postMessageRepository.AddAsync(postMessage);

            await _dbContext.SaveChangesAsync();
            //await _postsRepository.CompleteAsync();

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
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Некорректный формат GUID"));
            }

            var existingPost = await _postsRepository.GetAsync(postId);

            if (existingPost == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Пост с таким ID не найден"));
            }

            existingPost.Code = request.Post.Code;
            existingPost.Name = request.Post.Name;
            existingPost.River = request.Post.River;
            //existingPost.IsKafkaMessageSended = false;

            _postsRepository.Update(existingPost);
            var postMessage = new PostMessage() { Id = existingPost.Id, Code = existingPost.Code, Name = existingPost.Name, River = existingPost.River, postStatus = PostStatus.Updated };
            await _postMessageRepository.AddAsync(postMessage);

            await _dbContext.SaveChangesAsync();
            //await _postsRepository.CompleteAsync();

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
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Некорректный формат GUID"));
            }

            var post = await _postsRepository.GetAsync(guid);

            if (post == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Пост с таким ID не найден"));
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

        public override async Task<GetManyResponse> GetMany(GetManyRequest request, ServerCallContext context)
        {
            var postPageInfo = await _postsRepository.GetManyAsync(request.PageNumber, 10, request.Substring);

            var posts = postPageInfo.posts;
            uint maxPage = postPageInfo.pagesCount;

            if (!posts.Any())
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Не найдено постов на запрашиваемой странице"));
            }

            GetManyResponse getManyResponse = new GetManyResponse();

            foreach (var post in posts)
            {
                getManyResponse.Posts.Add(new Post
                {
                    Id = post.Id.ToString(),
                    Name = post.Name,
                    Code = post.Code,
                    River = post.River
                });
            }

            getManyResponse.PageNumber = request.PageNumber;
            getManyResponse.MaxPageNumber = maxPage;

            return getManyResponse;
        }

    }
}
