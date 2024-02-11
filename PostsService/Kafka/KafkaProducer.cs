using Confluent.Kafka;

namespace PostsService.Kafka
{
    public class KafkaProducer:IKafkaProducer
    {
        private readonly IProducer<Null,string> _producer;
        public KafkaProducer(ProducerConfig producerConfig)
        {
            _producer = new ProducerBuilder<Null,string>(producerConfig).Build();
        }

        public async Task SendMessage(string topic,string message,CancellationToken token)
        {
            var kafkaMessage = new Message<Null, string> { Value = message };
            await _producer.ProduceAsync(topic, kafkaMessage);
        }
    }
}
