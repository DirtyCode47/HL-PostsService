using PostsService.Entities;

namespace PostsService.Services.BackgroundKafkaSender
{
    public class Message<T> where T : class, IPosts
    {
        public string header { get; set; }
        public T body { get; set; }
    }
}
