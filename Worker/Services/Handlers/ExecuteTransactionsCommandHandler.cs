using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Worker
{
    public class ExecuteTransactionsCommandHandler : IRequestHandler<ExecuteTransactionsCommand>
    {
        private ILogger<ExecuteTransactionsCommandHandler> _logger;
        private ILedgerRepository _repo;

        public ExecuteTransactionsCommandHandler(ILogger<ExecuteTransactionsCommandHandler> logger, ILedgerRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<Unit> Handle(ExecuteTransactionsCommand command, CancellationToken cancellation)
        {
            foreach (var transaction in command.RecurringTransactions)
            {
                var entry = new LedgerEntry()
                {
                    UserId = transaction.UserId,
                    Category = transaction.Category,
                    Description = transaction.Description,
                    Amount = transaction.Amount,
                    TransactionTypeId = transaction.TransactionTypeId,
                    RecurringTransactionId = transaction.Id,
                    TransactionDate = DateTime.Now,
                    CreatedDate = DateTime.Now
                };
                // TODO (alexa): insert entry.
                // TODO (alexa): update transaction LastExecuted.
            }
            return Unit.Value;
        }
    }
}