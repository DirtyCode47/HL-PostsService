using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PostsService.Entities;
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

namespace PostsService.Services
{
    public class PostsServiceImpl : Protos.PostsService.PostsServiceBase
    {
        private readonly PostsServiceDbContext _dbContext;
        private readonly IPostsRepository _postsRepository;
        private readonly IPostMessageRepository _postMessageRepository;
        public PostsServiceImpl(PostsRepository postsRepository, IKafkaProducer kafkaProducer, PostsServiceDbContext dbContext, IPostMessageRepository postMessageRepository)
        {
            _dbContext = dbContext;
            _postsRepository = postsRepository;
            _postMessageRepository = postMessageRepository;
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            Guid postId = Guid.NewGuid();
            Posts post = new Posts() { Id = postId, Code = request.Code, Name = request.Name, River = request.River};

            if (await _postsRepository.GetAsync(postId) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Record with this id already exists in the database"));
            }

            if (await _postsRepository.FindByCodeAsync(request.Code) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Record with this post code already exists in the database"));
            }

            Posts addedPost = await _postsRepository.AddAsync(post);

            PostMessage postMessage = new PostMessage() { Id = addedPost.Id, Code = addedPost.Code, Name = addedPost.Name, River = addedPost.River, postStatus = PostStatus.Added };
            await _postMessageRepository.AddAsync(postMessage);

            await _dbContext.SaveChangesAsync();

            return new CreateResponse { Post = new Post { Id = post.Id.ToString(), Code = post.Code, Name = post.Name, River = post.River} };
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
                throw new RpcException(new Status(StatusCode.NotFound, "Can't find a record in the database with this id"));
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
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Uncorrect guid format"));
            }

            var existingPost = await _postsRepository.GetAsync(postId);

            if (existingPost == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find a record in the database with this id"));
            }

            if(await _postsRepository.FindByCodeAsync(request.Post.Code) != null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Post with such code already exists in DB"));
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
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Uncorrect guid format"));
            }

            var post = await _postsRepository.GetAsync(guid);

            if (post == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Can't find a record in the database with this id"));
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

        public override async Task<GetPageResponse> GetPage(GetPageRequest request, ServerCallContext context)
        {
            var postPageInfo = await _postsRepository.GetPageAsync(request.PageNumber, 10);

            var postPage = postPageInfo.postPage;
            uint maxPage = postPageInfo.maxPage;

            if (!postPage.Any())
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find elements on this page"));
            }

            GetPageResponse getPageResponse = new GetPageResponse();

            foreach (var post in postPage)
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

            return getPageResponse;
        }

        public override async Task<FindResponse> Find(FindRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Substring))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request has an empty string"));
            }

            var FindedPosts = await _postsRepository.FindWithSubstring(request.Substring);

            var responsePosts = new List<Post>();

            foreach (var post in FindedPosts)
            {
                responsePosts.Add(new Post
                {
                    Id = post.Id.ToString(),
                    Name = post.Name,
                    Code = post.Code,
                    River = post.River
                });
            }

            FindResponse response = new FindResponse();
            response.Posts.AddRange(responsePosts);

            return response;
        }
    }
}
