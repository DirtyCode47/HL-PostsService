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

namespace PostsService.Services
{
    public class PostsServiceImpl : Protos.PostsService.PostsServiceBase
    {
        private readonly PostsRepository _postsRepository;
        private readonly IKafkaProducer _kafkaProducer;
        public PostsServiceImpl(PostsRepository postsRepository, IKafkaProducer kafkaProducer)
        {
            _postsRepository = postsRepository ?? throw new ArgumentNullException(nameof(postsRepository));
            _kafkaProducer = kafkaProducer ?? throw new ArgumentNullException(nameof(kafkaProducer));
        }

        public override async Task<CreateResponse> Create(CreateRequest request, ServerCallContext context)
        {
            Guid postId = Guid.NewGuid();
            Posts post = new Posts() { Id = postId, Code = request.Post.Code, Name = request.Post.Name, River = request.Post.River, IsKafkaMessageSended = false };

            if (await _postsRepository.GetAsync(postId) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Record with this id already exists in the database"));
            }

            if (await _postsRepository.FindByCodeAsync(request.Post.Code) != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Record with this post code already exists in the database"));
            }

            Posts addedPost = await _postsRepository.AddAsync(post);
            await _postsRepository.CompleteAsync();

            //try
            //{
            //    if (_kafkaProducer != null)
            //    {
            //        string JsonAddedPost = System.Text.Json.JsonSerializer.Serialize(addedPost);
            //        await _kafkaProducer.SendMessage("posts", JsonAddedPost);
            //    }
            //    else
            //    {
            //        var messageWithPost = new MessageRetryPosts() { Id = addedPost.Id, Code = addedPost.Code, Name = addedPost.Name, River = addedPost.River };
            //        await _messageRetryPostsRepository.AddAsync(messageWithPost);
            //        await _messageRetryPostsRepository.CompleteAsync();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var messageWithPost = new MessageRetryPosts() { Id = addedPost.Id, Code = addedPost.Code, Name = addedPost.Name, River = addedPost.River };
            //    await _messageRetryPostsRepository.AddAsync(messageWithPost);
            //    await _messageRetryPostsRepository.CompleteAsync();

            //    return new CreateResponse { Post = request.Post };
            //}

            //string JsonAddedPost = System.Text.Json.JsonSerializer.Serialize(addedPost);


            //var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

            //using (var producer = new ProducerBuilder<Null, string>(config).Build())
            //{
            //    var message = new Message<Null, string> { Value = JsonAddedPost };
            //    var deliveryReport = await producer.ProduceAsync("posts", message);
            //}

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
            existingPost.IsKafkaMessageSended = false;

            _postsRepository.Update(existingPost);
            await _postsRepository.CompleteAsync();

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
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find a record in the database with this id"));
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

        public override Task<FindResponse> Find(FindRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Substring))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Request has an empty string"));
            }

            string lower_substring = request.Substring.ToLower();
            List<string> words = lower_substring.Split(' ').ToList();

            List<Posts> posts = _postsRepository.GetAllPosts().ToList();
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

        public override async Task<GetAllResponse> GetAll(GetAllRequest request, ServerCallContext context)
        {
            
            var posts = await _postsRepository.GetAllPostsAsync(); 
            

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

            return response;
        }
    }
}
