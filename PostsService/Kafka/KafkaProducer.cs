using Confluent.Kafka;
using PostsService.Protos;
using PostsService.Entities;

namespace PostsService.Kafka
{
    public class KafkaProducer:IKafkaProducer
    {
        private readonly IProducer<Null,string> _producer;
        public KafkaProducer(ProducerConfig producerConfig, IConfiguration configuration)
        {
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task SendMessage<T>(string topic, T message, CancellationToken token)
        {
            string serializedMessage = System.Text.Json.JsonSerializer.Serialize(message);

            var kafkaMessage = new Message<Null, string> { Value = serializedMessage };
            await _producer.ProduceAsync(topic, kafkaMessage);
        }
    }
}
