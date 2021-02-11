using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Serilog;
using Worker;

namespace Tests
{
    public class ExecuteTransactionsCommandHandlerShould
    {
        private string _categoryExists = Guid.NewGuid().ToString();
        private string _categoryNotExists = Guid.NewGuid().ToString();
        private Mock<ILedgerRepository> _repo;
        private ExecuteTransactionsCommandHandler _handler;

        public ExecuteTransactionsCommandHandlerShould()
        {
            IEnumerable<LedgerEntryCategory> categories = new List<LedgerEntryCategory>(){
                new LedgerEntryCategory()
                {
                    Id = Guid.NewGuid().ToString(),
                    Category = _categoryExists
                }
            };
            IEnumerable<LedgerEntryCategory> noCategories = new List<LedgerEntryCategory>();

            _repo = new Mock<ILedgerRepository>();
            _repo.Setup(x => x.GetCategoriesByCategoryAsync(_categoryExists))
                .Returns(Task.FromResult(categories));
            _repo.Setup(x => x.GetCategoriesByCategoryAsync(_categoryNotExists))
                .Returns(Task.FromResult(noCategories));

            _handler = new ExecuteTransactionsCommandHandler(new Mock<ILogger>().Object, _repo.Object);
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

        [Fact]
        public async Task InsertsNewCategory()
        {
            var transaction = CreateRecurringTransaction();
            transaction.Category = _categoryNotExists;

            var command = new ExecuteTransactionsCommand()
            {
                RecurringTransactions = new List<RecurringTransaction>() { transaction }
            };

            await _handler.Handle(command, new CancellationToken());

            _repo.Verify(x => x.GetCategoriesByCategoryAsync(_categoryNotExists), Times.Once);
            _repo.Verify(x => x.InsertLedgerEntryCategoryAsync(It.IsAny<LedgerEntryCategory>()), Times.Once);
        }

        [Fact]
        public async Task UpdatesExistingCategory()
        {
            var transaction = CreateRecurringTransaction();
            transaction.Category = _categoryExists;

            var command = new ExecuteTransactionsCommand()
            {
                RecurringTransactions = new List<RecurringTransaction>() { transaction }
            };

            await _handler.Handle(command, new CancellationToken());

            _repo.Verify(x => x.GetCategoriesByCategoryAsync(_categoryExists), Times.Once);
            _repo.Verify(x => x.UpdateCategoryLastUsedAsync(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
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