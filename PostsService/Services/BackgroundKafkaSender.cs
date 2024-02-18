using PostsService.Kafka;
using PostsService.Repositories;
using PostsService.Entities;

namespace PostsService.Services
{
    public class BackgroundKafkaSender : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly KafkaProducer _producer;

        public BackgroundKafkaSender(IServiceScopeFactory scopeFactory, KafkaProducer kafkaProducer)
        {
            _producer = kafkaProducer;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _postsRepository = scope.ServiceProvider.GetRequiredService<PostsRepository>();

                    List<Posts> failedMessages = await _postsRepository.GetUnsentKafkaMessagesAsync();

                    foreach (var message in failedMessages)
                    {
                        try
                        {
                            var cts = new CancellationTokenSource();
                            cts.CancelAfter(TimeSpan.FromSeconds(5));

                            var cancellationToken = cts.Token;
                            await _producer.SendMessage("posts", System.Text.Json.JsonSerializer.Serialize(message), cancellationToken);

                            message.IsKafkaMessageSended = true;
                            _postsRepository.Update(message);
                            await _postsRepository.CompleteAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error retrying message: {0}", message.Id);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Например, пауза в 5 минут
            }
        }
    }
}
