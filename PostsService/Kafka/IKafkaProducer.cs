using PostsService.Entities;
using PostsService.Services.BackgroundKafkaSender;
namespace PostsService.Kafka
{
    public interface IKafkaProducer
    {
        public Task SendMessage<T>(string topic, Message<T> message, CancellationToken token) where T : class, IPosts;
    }
}
