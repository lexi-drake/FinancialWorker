using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Worker
{
    public class UserDeleter : BackgroundService
    {
        private const int PERIOD_HOURS = 24;
        private const int PERIOD_MILLISECONDS = 1000 * 60 * 60 * PERIOD_HOURS;
        private ILogger<UserDeleter> _logger;
        private IMediator _mediatr;

        public UserDeleter(ILogger<UserDeleter> logger, IMediator mediatr)
        {
            _logger = logger;
            _mediatr = mediatr;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            Console.WriteLine($"loglevel info: {_logger.IsEnabled(LogLevel.Information)}");
            _logger.LogInformation("UserDeleter beginning ExecuteAsync");
            while (!cancellation.IsCancellationRequested)
            {
                DeleteExpiredUsers()
                    .ContinueWith(task =>
                    {
                        _logger.LogInformation("Deleting expired users complete.");
                    }, TaskContinuationOptions.None)
                    .ContinueWith(task =>
                    {
                        _logger.LogError(task.Exception.Message);
                    }, TaskContinuationOptions.OnlyOnFaulted);
                await Task.Delay(PERIOD_MILLISECONDS, cancellation);
            }
        }

        private async Task DeleteExpiredUsers()
        {
            _logger.LogInformation("Deleting expired users starting.");
            var expiredUserIds = await _mediatr.Send(new GetExpiredUsersQuery());
            if (!expiredUserIds.Any())
            {
                _logger.LogInformation("No expired users.");
                return;
            }

            var ledgerDataCommand = new DeleteLedgerDataForUsersCommand() { UserIds = expiredUserIds };
            await _mediatr.Send(ledgerDataCommand);

            var userCommand = new DeleteUsersCommand() { UserIds = expiredUserIds };
            await _mediatr.Send(userCommand);
        }
    }
}