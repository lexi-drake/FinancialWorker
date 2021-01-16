using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MediatR;


namespace Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    services.AddScoped<ILedgerRepository>(s => new LedgerRepository(
                        configuration["MONGO_DB"],
                        configuration["LEDGER_DB"]));
                    services.AddScoped<IUserRespository>(s => new UserRepository(
                        configuration["MONGO_DB"],
                        configuration["USERS_DB"]));

                    services.AddMediatR(typeof(Program));
                    services.AddHostedService<UserDeleter>();
                    services.AddHostedService<TransactionExecuter>();
                });
    }
}
