using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Worker
{
    // https://ell.stackexchange.com/questions/19860/executer-vs-executor
    public class TransactionExecuter : BackgroundService
    {
        private const int PERIOD_HOURS = 3;
        private const int PERIOD_MILLISECONDS = 1000 * 60 * 60 * PERIOD_HOURS;
        private ILogger<TransactionExecuter> _logger;
        private IMediator _mediatr;

        public TransactionExecuter(ILogger<TransactionExecuter> logger, IMediator mediatr)
        {
            _logger = logger;
            _mediatr = mediatr;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                ExecuteTransactions()
                    .ContinueWith(task =>
                    {
                        _logger.LogInformation($"Executing transactions complete at {DateTime.Now}.");
                    }, TaskContinuationOptions.None)
                    .ContinueWith(task =>
                    {
                        _logger.LogError(task.Exception.Message);
                    }, TaskContinuationOptions.OnlyOnFaulted);
                await Task.Delay(PERIOD_MILLISECONDS, cancellation);
            }
        }

        private async Task ExecuteTransactions()
        {
            _logger.LogInformation($"Executing transactions starting at {DateTime.Now}.");
            var transactions = await _mediatr.Send(new GetDueTransactionsQuery());
            if (!transactions.Any())
            {
                _logger.LogInformation("No transactions due.");
            }
            var command = new ExecuteTransactionsCommand() { RecurringTransactions = transactions };
            await _mediatr.Send(command);
        }
    }
}