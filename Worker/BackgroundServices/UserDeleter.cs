using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MediatR;
using Serilog;

namespace Worker
{
    public class UserDeleter : BackgroundService
    {
        private const int PERIOD_HOURS = 24;
        private const int PERIOD_MILLISECONDS = 1000 * 60 * 60 * PERIOD_HOURS;
        private ILogger _logger;
        private IMediator _mediatr;

        public UserDeleter(ILogger logger, IMediator mediatr)
        {
            _logger = logger;
            _mediatr = mediatr;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.Information("UserDeleter beginning ExecuteAsync");
            while (!cancellation.IsCancellationRequested)
            {
                var task = DeleteExpiredUsers()
                    .ContinueWith(task =>
                    {
                        _logger.Information("Deleting expired users complete.");
                    }, TaskContinuationOptions.None)
                    .ContinueWith(task =>
                    {
                        _logger.Error(task.Exception.Message);
                    }, TaskContinuationOptions.OnlyOnFaulted);
                await Task.Delay(PERIOD_MILLISECONDS, cancellation);
            }
        }

        private async Task DeleteExpiredUsers()
        {
            _logger.Information("Deleting expired users starting.");
            var expiredUserIds = await _mediatr.Send(new GetExpiredUsersQuery());
            if (!expiredUserIds.Any())
            {
                _logger.Information("No expired users.");
                return;
            }

            var ledgerDataCommand = new DeleteLedgerDataForUsersCommand() { UserIds = expiredUserIds };
            await _mediatr.Send(ledgerDataCommand);

            var userCommand = new DeleteUsersCommand() { UserIds = expiredUserIds };
            await _mediatr.Send(userCommand);
        }
    }
}