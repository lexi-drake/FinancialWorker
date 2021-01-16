using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Worker;

namespace Tests
{
    public class DeleteLedgerDataForUsersCommandHandlerShould
    {
        private string _userId = Guid.NewGuid().ToString();
        private long _entryCount;
        private long _transactionCount;
        private long _generatorCount;
        private Mock<ILogger<DeleteLedgerDataForUsersCommandHandler>> _logger;
        private DeleteLedgerDataForUsersCommandHandler _handler;

        public DeleteLedgerDataForUsersCommandHandlerShould()
        {
            var random = new Random();
            _entryCount = random.Next();
            _transactionCount = random.Next();
            _generatorCount = random.Next();

            _logger = new Mock<ILogger<DeleteLedgerDataForUsersCommandHandler>>();

            var repo = new Mock<ILedgerRepository>();
            repo.Setup(x => x.DeleteLedgerEntriesByUserIdAsync(_userId))
                .Returns(Task.FromResult(_entryCount));
            repo.Setup(x => x.DeleteRecurringTransactionsByUserIdAsync(_userId))
                .Returns(Task.FromResult(_transactionCount));
            repo.Setup(x => x.DeleteIncomeGeneratorsByUserIdAsync(_userId))
                .Returns(Task.FromResult(_generatorCount));

            _handler = new DeleteLedgerDataForUsersCommandHandler(_logger.Object, repo.Object);
        }

        [Fact]
        public async Task DeletesAndLogs()
        {
            var command = new DeleteLedgerDataForUsersCommand()
            {
                UserIds = new List<string>() { _userId }
            };

            await _handler.Handle(command, new CancellationToken());

            _logger.VerifyLoggedInformation($"Deleting ledger data for user {_userId}.");
            _logger.VerifyLoggedInformation($"Deleted {_entryCount} ledger entries, {_transactionCount} recurring transactions, and {_generatorCount} income generators.");
        }
    }
}