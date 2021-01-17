using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MediatR;
using Serilog;


namespace Worker
{
    public class Program
    {
        private const string APPSETTINGS = "appsettings.json";
        private static IConfiguration Configuration;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    if (File.Exists(APPSETTINGS))
                    {
                        config.AddJsonFile(APPSETTINGS);
                    }
                    else
                    {
                        config.AddEnvironmentVariables();
                    }
                    Configuration = config.Build();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    ILogger logger = new LoggerConfiguration()
                        .MinimumLevel.Information()
                        .WriteTo.File("./log.log")
                        .WriteTo.Console()
                        .CreateLogger();
                    services.AddScoped<ILogger>(s => logger);

                    services.AddScoped<ILedgerRepository>(s => new LedgerRepository(
                        Configuration["MONGO_DB"],
                        Configuration["LEDGER_DB"]));
                    services.AddScoped<IUserRespository>(s => new UserRepository(
                        Configuration["MONGO_DB"],
                        Configuration["USERS_DB"]));

                    services.AddMediatR(typeof(Program));
                    services.AddHostedService<UserDeleter>();
                    services.AddHostedService<TransactionExecuter>();
                });
    }
}
