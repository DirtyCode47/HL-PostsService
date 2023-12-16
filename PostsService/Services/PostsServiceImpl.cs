
using Grpc.Core;
using Npgsql;
using PostsService.Exceptions;
using PostsService.Protos;
using PostsService.Repositories;
using System.Data.Common;
using PostsService.Entities;
using PostsService.Protos;

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
            return Task.FromResult(new CreateResponse { });
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
