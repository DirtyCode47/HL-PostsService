
using Grpc.Core;
using Npgsql;
using PostsService.Exceptions;
using PostsService.Protos;
using PostsService.Repositories;
using System.Data.Common;
using PostsService.Entities;
using PostsService.Protos;
using System.ComponentModel.DataAnnotations;

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
            try
            {
                Posts added_post = postsRepository.Add(post);
                postsRepository.Complete();
                return Task.FromResult(new CreateResponse { Post = request.Post });
            }
            catch (ExistRecordInDbException ex)
            {
                Console.WriteLine(ex.Message);
                throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
            }
            catch (ValidationException ex)
            {
                Console.WriteLine(ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (PostgresException ex)
            {
                Console.WriteLine(ex.Message);
                throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
            }
            catch (DbException ex)
            {
                Console.WriteLine(ex.Message);
                throw new RpcException(new Status(StatusCode.Unavailable, ex.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new RpcException(new Status(StatusCode.Unknown, ex.Message));
            }
        }
        public override Task<DeleteResponse> Delete(DeleteRequest request, ServerCallContext context)
        {
            return Task.FromResult(new DeleteResponse { });
        }
        public override Task<UpdateResponse> Update(UpdateRequest request, ServerCallContext context)
        {
            return Task.FromResult(new UpdateResponse { });
        }
        public override Task<GetResponse> Get(GetRequest request, ServerCallContext context)
        {
            Guid guid = Guid.Parse(request.Id);
            try
            {
                Posts post = postsRepository.Get(guid);

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
            catch (NoSuchRecordInDbException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (PostgresException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch(DbException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            //return Task.FromResult(new GetResponse { });
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
