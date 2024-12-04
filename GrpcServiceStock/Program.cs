using GrpcServiceStock;
using GrpcServiceStock.Common;
using GrpcServiceStock.SQL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrpcServiceLab
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConfigurationHelper.Configuration();

            SqlData.Int();

            OnlineManager.Init();

            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
             Host.CreateDefaultBuilder(args).UseSystemd()
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder.UseStartup<Startup>();
                 webBuilder.UseUrls("https://+:5001", "http://+:5000");
             });
    }
}
