using PostsService.Kafka;
using Microsoft.EntityFrameworkCore;
using PostsService.Entities.PostMessage;
using PostsService.Repositories.PostMessageRepository;

namespace PostsService.Services.BackgroundKafkaSender
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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var postMessageRepository = scope.ServiceProvider.GetRequiredService<IPostMessageRepository>();

                    List<PostMessage> unloadedPosts = await postMessageRepository.GetAll().ToListAsync();

                    foreach (var post in unloadedPosts)
                    {
                        try
                        {
                            var cts = new CancellationTokenSource();
                            cts.CancelAfter(TimeSpan.FromSeconds(5));

                            var cancellationToken = cts.Token;

                            string messageHeader = post.postStatus switch
                            {
                                PostStatus.Added => $"{post.Code} added",
                                PostStatus.Updated => $"{post.Code} updated",
                                PostStatus.Deleted => $"{post.Code} deleted",
                                _ => "Something wrong happened!"
                            };

                            var message = new Message<PostMessage>() { header = messageHeader, body = post };

                            await _producer.SendMessage(_configuration.GetSection("PostMessagesTopic").Value, message, cancellationToken);

                            postMessageRepository.Delete(post);
                            await postMessageRepository.CompleteAsync();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error retrying message: {0}", post.Id);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Например, пауза в 5 минут
            }
        }
    }
}
