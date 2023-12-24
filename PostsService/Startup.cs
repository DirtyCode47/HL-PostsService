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
using PostsService.Cache;

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
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var configuration = ConfigurationOptions.Parse(Configuration.GetConnectionString("RedisConnection"));
                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddDbContext<PostsServiceDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<PostsRepository>();
            services.AddScoped<PostsServiceImpl>();
            services.AddScoped<CacheService>();

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

            app.Use(async (context, next) =>
            {
                // Получаем сервис провайдер из контекста запроса
                var serviceProvider = context.RequestServices;

                // Получаем scoped-сервис PostsRepository
                var postsRepository = serviceProvider.GetRequiredService<PostsRepository>();

                // Получаем подключение к Redis
                var connectionMultiplexer = serviceProvider.GetRequiredService<IConnectionMultiplexer>();

                // Инициализируем кэш асинхронно
                await InitializeCacheAsync(postsRepository, connectionMultiplexer);

                // Передаем управление следующему middleware
                await next();
            });

            app.UseRouting();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<PostsServiceImpl>();
            });
        }
        private async Task InitializeCacheAsync(PostsRepository postsRepository, IConnectionMultiplexer connectionMultiplexer)
        {
            // Получаем данные из репозитория
            var posts = postsRepository.GetAllPosts();

            // Подключение к Redis
            var database = connectionMultiplexer.GetDatabase();

            // Проходим по всем записям и добавляем/обновляем данные в кэше
            foreach (var post in posts)
            {
                var cacheKey = $"post:{post.Id}";

                // Преобразовываем объект в JSON (или любой другой формат)
                var serializedPost = JsonConvert.SerializeObject(post);

                // Записываем данные в Redis
                await database.StringSetAsync(cacheKey, serializedPost);
            }
        }
    }
}
