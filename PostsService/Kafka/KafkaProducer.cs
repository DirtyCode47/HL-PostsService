using Confluent.Kafka;
using PostsService.Protos;
using PostsService.Entities;
using PostsService.Services.BackgroundKafkaSender;

namespace PostsService.Kafka
{
    public class KafkaProducer:IKafkaProducer
    {
        private readonly IProducer<Null,string> _producer;
        public KafkaProducer(ProducerConfig producerConfig, IConfiguration configuration)
        {
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task SendMessage<T>(string topic, Message<T> message, CancellationToken token) where T : class,IPosts,ISerializableObject
        {
            string serializedMessage= message.SerializeToJson();

            var kafkaMessage = new Message<Null, string> { Value = serializedMessage };
            await _producer.ProduceAsync(topic, kafkaMessage);
        }
    }
}
