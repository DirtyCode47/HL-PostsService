
using Grpc.Core;
using PostsService.Protos;
using PostsService.Repositories;

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
            return Task.FromResult(new GetResponse { });
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
