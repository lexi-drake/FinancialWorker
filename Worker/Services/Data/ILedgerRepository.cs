using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worker
{
    public interface ILedgerRepository
    {
        Task<IEnumerable<LedgerEntryCategory>> GetCategoriesByCategoryAsync(string category);
        Task<LedgerEntryCategory> InsertLedgerEntryCategoryAsync(LedgerEntryCategory category);
        Task UpdateCategoryLastUsedAsync(string id, DateTime lastUsed);
        Task<long> DeleteLedgerEntriesByUserIdAsync(string userId);
        Task<long> DeleteRecurringTransactionsByUserIdAsync(string userId);
        Task<long> DeleteIncomeGeneratorsByUserIdAsync(string userId);
        Task<IEnumerable<Frequency>> GetFrequenciesAsync();
        Task<IEnumerable<RecurringTransaction>> GetRecurringTransactionsByFrequencyAndLastExecutedAsync(string frequencyId, DateTime since);
        Task InsertLedgerEntryAsync(LedgerEntry entry);
        Task UpdateRecurringTransactionLastExecutedAsync(string id, DateTime lastExecuted);
        Task UpdateRecurringTransactionLastTriggeredAsync(string id, DateTime lastTriggered);
    }
}