using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Worker;

namespace Tests
{
    public class ExecuteTransactionsCommandHandlerShould
    {
        private Mock<ILedgerRepository> _repo;
        private ExecuteTransactionsCommandHandler _handler;

        public ExecuteTransactionsCommandHandlerShould()
        {
            _repo = new Mock<ILedgerRepository>();

            _handler = new ExecuteTransactionsCommandHandler(new Mock<ILogger<ExecuteTransactionsCommandHandler>>().Object, _repo.Object);
        }

        [Fact]
        public async Task InsertsLedgerEntriesAndUpdatesLastExecuted()
        {
            var count = new Random().Next(1000);
            var transactions = new List<RecurringTransaction>();
            for (var i = 0; i < count; i++)
            {
                transactions.Add(CreateRecurringTransaction());
            }

            var command = new ExecuteTransactionsCommand()
            {
                RecurringTransactions = transactions
            };

            await _handler.Handle(command, new CancellationToken());

            _repo.Verify(x => x.InsertLedgerEntryAsync(It.IsAny<LedgerEntry>()), Times.Exactly(count));
            _repo.Verify(x => x.UpdateRecurringTransactionLastExecutedAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(count));
        }

        private RecurringTransaction CreateRecurringTransaction()
        {
            return new RecurringTransaction()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString()
            };
        }
    }
}