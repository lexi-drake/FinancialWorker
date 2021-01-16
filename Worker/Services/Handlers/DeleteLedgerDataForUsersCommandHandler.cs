using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Worker
{
    public class DeleteLedgerDataForUsersCommandHandler : IRequestHandler<DeleteLedgerDataForUsersCommand>
    {
        private ILogger<DeleteLedgerDataForUsersCommandHandler> _logger;
        private ILedgerRepository _repo;

        public DeleteLedgerDataForUsersCommandHandler(ILogger<DeleteLedgerDataForUsersCommandHandler> logger, ILedgerRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<Unit> Handle(DeleteLedgerDataForUsersCommand command, CancellationToken cancellation)
        {
            foreach (var id in command.UserIds)
            {
                _logger.LogInformation($"Deleting ledger data for user {id}.");
                var entryCount = await _repo.DeleteLedgerEntriesByUserIdAsync(id);
                var transactionCount = await _repo.DeleteRecurringTransactionsByUserIdAsync(id);
                var generatorCount = await _repo.DeleteIncomeGeneratorsByUserIdAsync(id);
                _logger.LogInformation($"Deleted {entryCount} ledger entries, {transactionCount} recurring transactions, and {generatorCount} income generators.");
            }
            return Unit.Value;
        }
    }
}