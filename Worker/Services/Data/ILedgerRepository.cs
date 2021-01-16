using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Worker
{
    public interface ILedgerRepository
    {
        Task<long> DeleteLedgerEntriesByUserIdAsync(string userId);
        Task<long> DeleteRecurringTransactionsByUserIdAsync(string userId);
        Task<long> DeleteIncomeGeneratorsByUserIdAsync(string userId);
        Task<IEnumerable<Frequency>> GetFrequenciesAsync();
        Task<IEnumerable<RecurringTransaction>> GetRecurringTransactionsByFrequencyAndLastExecutedAsync(string frequencyId, DateTime since);
    }
}