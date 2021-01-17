using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Serilog;
using Worker;

namespace Tests
{
    public class GetDueTransactionsQueryHandlerShould
    {
        private IEnumerable<Frequency> _frequencies;
        private Mock<ILedgerRepository> _repo;
        private GetDueTransactionsQueryHandler _handler;

        public GetDueTransactionsQueryHandlerShould()
        {
            _frequencies = CreateFrequencies();
            _repo = new Mock<ILedgerRepository>();
            _repo.Setup(x => x.GetFrequenciesAsync())
                .Returns(Task.FromResult(_frequencies));
            _repo.Setup(x => x.GetRecurringTransactionsByFrequencyAndLastExecutedAsync(It.IsAny<string>(), It.IsAny<DateTime>()))
                .Returns(Task.FromResult(GetListOfRecurringTransactions()));

            _handler = new GetDueTransactionsQueryHandler(new Mock<ILogger>().Object, _repo.Object);
        }

        [Fact]
        public async Task UpdatesLastTriggered()
        {
            var query = new GetDueTransactionsQuery();

            var transactions = await _handler.Handle(query, new CancellationToken());

            _repo.Verify(x => x.GetRecurringTransactionsByFrequencyAndLastExecutedAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(_frequencies.Count()));
            _repo.Verify(x => x.UpdateRecurringTransactionLastTriggeredAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(transactions.Count()));
        }

        private IEnumerable<RecurringTransaction> GetListOfRecurringTransactions()
        {
            var count = new Random().Next(1000);
            var transactions = new List<RecurringTransaction>();
            for (var i = 0; i < count; i++)
            {
                transactions.Add(new RecurringTransaction()
                {
                    Id = Guid.NewGuid().ToString()
                });
            }
            return transactions;
        }

        private IEnumerable<Frequency> CreateFrequencies()
        {
            var days = new int[] { 1, 2, 4, 12, 26, 52 };
            var frequencies = new List<Frequency>();
            foreach (var day in days)
            {
                frequencies.Add(new Frequency()
                {
                    Id = Guid.NewGuid().ToString(),
                    ApproxTimesPerYear = day,
                    Description = Guid.NewGuid().ToString()
                });
            }
            return frequencies;
        }
    }
}