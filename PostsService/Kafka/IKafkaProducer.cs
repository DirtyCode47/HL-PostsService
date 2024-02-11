namespace PostsService.Kafka
{
    public interface IKafkaProducer
    {
        public Task SendMessage(string topic, string message,CancellationToken cancellationToken);
    }
}
