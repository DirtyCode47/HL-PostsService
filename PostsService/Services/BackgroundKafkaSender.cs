using PostsService.Kafka;
using PostsService.Repositories;
using PostsService.Entities;

namespace PostsService.Services
{
    public class BackgroundKafkaSender : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly IKafkaProducer _producer;

        public BackgroundKafkaSender(IServiceScopeFactory scopeFactory, IKafkaProducer kafkaProducer, IConfiguration configuration)
        {
            _producer = kafkaProducer;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //using (var scope = _scopeFactory.CreateScope())
                //{
                //    var _postsRepository = scope.ServiceProvider.GetRequiredService<PostsRepository>();

                //    List<Posts> failedMessages = await _postsRepository.FindUnloadedPostsAsync();

                //    foreach (var message in failedMessages)
                //    {
                //        try
                //        {
                //            var cts = new CancellationTokenSource();
                //            cts.CancelAfter(TimeSpan.FromSeconds(5));

                //            var cancellationToken = cts.Token;
                //            await _producer.SendMessage(_configuration.GetSection("PostMessagesTopic").Value, message, cancellationToken);

                //            message.IsKafkaMessageSended = true;
                //            _postsRepository.Update(message);
                //            await _postsRepository.CompleteAsync();
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine($"Error retrying message: {0}", message.Id);
                //        }
                //    }
                //}

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Например, пауза в 5 минут
            }
        }
    }
}
