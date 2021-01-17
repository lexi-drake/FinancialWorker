using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MediatR;
using Serilog;

namespace Worker
{
    // https://ell.stackexchange.com/questions/19860/executer-vs-executor
    public class TransactionExecuter : BackgroundService
    {
        private const int PERIOD_HOURS = 6;
        private const int PERIOD_MILLISECONDS = 1000 * 60 * 60 * PERIOD_HOURS;
        private ILogger _logger;
        private IMediator _mediatr;

        public TransactionExecuter(ILogger logger, IMediator mediatr)
        {
            _logger = logger;
            _mediatr = mediatr;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.Information("TransactionExecuter beginning ExecuteAsync");
            while (!cancellation.IsCancellationRequested)
            {
                ExecuteTransactions()
                    .ContinueWith(task =>
                    {
                        _logger.Information("Executing transactions complete.");
                    }, TaskContinuationOptions.None)
                    .ContinueWith(task =>
                    {
                        _logger.Error(task.Exception.Message);
                    }, TaskContinuationOptions.OnlyOnFaulted);
                await Task.Delay(PERIOD_MILLISECONDS, cancellation);
            }
        }

        private async Task ExecuteTransactions()
        {
            _logger.Information("Executing transactions starting.");
            var transactions = await _mediatr.Send(new GetDueTransactionsQuery());
            if (!transactions.Any())
            {
                _logger.Information("No transactions due.");
            }
            var command = new ExecuteTransactionsCommand() { RecurringTransactions = transactions };
            await _mediatr.Send(command);
        }
    }
}