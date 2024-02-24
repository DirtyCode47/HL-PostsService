using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using PostsService.Kafka;
using PostsService.Repositories;
using PostsService.Repositories.PostMessageRepository;
using PostsService.Repositories.PostsRepository;
using PostsService.Services.BackgroundKafkaSender;
using PostsService.Services.PostsServiceImpl;
//using PostsService.Repositories.PostsRepository;

namespace PostsService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
      
            services.AddDbContext<PostsServiceDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<PostsRepository>();
            services.AddScoped<PostsServiceImpl>();
            services.AddScoped<IPostsRepository, PostsRepository>();
            services.AddScoped<IPostMessageRepository, PostMessagesRepository>();

            services.AddHostedService<BackgroundKafkaSender>();

            services.AddSingleton(new ProducerConfig
            {
                BootstrapServers = Configuration.GetConnectionString("BootstrapServersConnection"),
                //Retries = 3,
                //RetryBackoffMaxMs = 1000,
                //MessageSendMaxRetries = 0
                //ReconnectBackoffMs
            });

            services.AddSingleton<KafkaProducer>();
            services.AddSingleton<IKafkaProducer, KafkaProducer>();

            services.AddGrpc();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseRouting();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<PostsServiceImpl>();
            });
        }
        
    }
}
