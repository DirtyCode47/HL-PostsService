namespace PostsService.Kafka
{
    public interface IKafkaProducer
    {
        public Task SendMessage<T>(string topic, T message, CancellationToken cancellationToken);
    }
}
