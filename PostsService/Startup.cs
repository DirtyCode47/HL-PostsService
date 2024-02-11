using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using PostsService.Repositories;
using PostsService.Services;
using StackExchange.Redis;
using Newtonsoft.Json;
using Confluent.Kafka;
using PostsService.Kafka;

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

            services.AddHostedService<BackgroundKafkaSender>();

            // В методе ConfigureServices
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
