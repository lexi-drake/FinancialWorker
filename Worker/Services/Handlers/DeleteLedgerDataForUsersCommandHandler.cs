using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Serilog;

namespace Worker
{
    public class DeleteLedgerDataForUsersCommandHandler : IRequestHandler<DeleteLedgerDataForUsersCommand>
    {
        private ILogger _logger;
        private ILedgerRepository _repo;

        public DeleteLedgerDataForUsersCommandHandler(ILogger logger, ILedgerRepository repo)
        {
            _logger = logger;
            _repo = repo;
        }

        public async Task<Unit> Handle(DeleteLedgerDataForUsersCommand command, CancellationToken cancellation)
        {
            foreach (var id in command.UserIds)
            {
                _logger.Information($"Deleting ledger data for user {id}.");
                var entryCount = await _repo.DeleteLedgerEntriesByUserIdAsync(id);
                var transactionCount = await _repo.DeleteRecurringTransactionsByUserIdAsync(id);
                var generatorCount = await _repo.DeleteIncomeGeneratorsByUserIdAsync(id);
                _logger.Information($"Deleted {entryCount} ledger entries, {transactionCount} recurring transactions, and {generatorCount} income generators.");
            }
            return Unit.Value;
        }
    }
}