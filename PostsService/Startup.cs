using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

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
            //var connectionString = Configuration.GetConnectionString("DefaultConnection");
            //Console.WriteLine($"ConnectionString: {connectionString}");



            //services.AddDbContext<HControlServiceDbContext>(options =>
            //    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            //services.AddScoped<Repository<HydrologyControl>>();
            //services.AddScoped<HydrologyControlServiceImpl>();

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
                //endpoints.MapGrpcService<HydrologyControlService>();


                //endpoints.MapGrpcService<HydrologyControlServiceImpl>();


                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello, this is an empty page!");
                //});
            });
        }
    }
}
