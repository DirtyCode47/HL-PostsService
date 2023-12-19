
using Grpc.Core;
using Npgsql;
using PostsService.Exceptions;
using PostsService.Protos;
using PostsService.Repositories;
using System.Data.Common;
using PostsService.Entities;
using PostsService.Protos;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using static Grpc.Core.Metadata;

namespace PostsService.Services
{
    public class PostsServiceImpl:Protos.PostsService.PostsServiceBase
    {
        PostsRepository postsRepository;
        public PostsServiceImpl(PostsRepository postsRepository) 
        {
            this.postsRepository = postsRepository;
        }
        public override Task<CreateResponse> Create(CreateRequest request,ServerCallContext context)
        {
            Posts post = new Posts() { Id = Guid.Parse(request.Post.Id), Code = request.Post.Code, Name = request.Post.Name, River = request.Post.River };

            if (postsRepository.Get(Guid.Parse(request.Post.Id)) != null) 
                throw new RpcException(new Status(StatusCode.AlreadyExists, "This record already exist in Db"));

            Posts added_post = postsRepository.Add(post);
            postsRepository.Complete();

            return Task.FromResult(new CreateResponse { Post = request.Post });
        }
        public override Task<DeleteResponse> Delete(DeleteRequest request, ServerCallContext context)
        {
            Posts entity = new Posts() { Id = Guid.Parse(request.Post.Id), Code = request.Post.Code, Name = request.Post.Name, River = request.Post.River };
            var entry = postsRepository.Delete(entity);

            if (entry == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find record in Db with this id"));

            postsRepository.Complete();

            return Task.FromResult(new DeleteResponse
            {
                Post = new Post
                {
                    Id = entity.Id.ToString(),
                    Code = entity.Code,
                    Name = entity.Name,
                    River = entity.River,
                }
            });
        }
        public override Task<UpdateResponse> Update(UpdateRequest request, ServerCallContext context)
        {
            return Task.FromResult(new UpdateResponse { });
        }
        public override Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            Guid guid = Guid.Parse(request.Id);
            var post = postsRepository.Get(guid);

            if (post == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Can't find record in Db with this id"));

            return Task.FromResult(new GetResponse
            {
                Post = new Post()
                {
                    Id = post.Id.ToString(),
                    Code = post.Code,
                    Name = post.Name,
                    River = post.River
                }
            });
        }
        public override Task<GetPageResponse> GetPage(GetPageRequest request, ServerCallContext context)
        {
            return Task.FromResult(new GetPageResponse { });
        }
        public override Task<FindResponse> Find(FindRequest request, ServerCallContext context)
        {
            return Task.FromResult(new FindResponse { });
        }
    }
}
