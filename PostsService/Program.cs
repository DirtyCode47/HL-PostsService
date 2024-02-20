using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PostsService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            var app = builder.Build();
            app.Run();
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var url = context.Configuration.GetValue<string>("MyAppConfig:Kestrel:Endpoints:Http:Url");

                        var uriBuilder = new UriBuilder(url);
                        var host = uriBuilder.Host;
                        var port = uriBuilder.Port;

                        options.ListenAnyIP(port, listenOptions =>
                        {
                            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                        });
                    });

                    webBuilder.UseStartup<Startup>();
                });
    }
}
