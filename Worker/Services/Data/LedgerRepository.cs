using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Worker
{
    public class LedgerRepository : ILedgerRepository
    {
        private IMongoDatabase _db;

        public LedgerRepository(string connectionString, string database)
        {
            var client = new MongoClient(connectionString);
            _db = client.GetDatabase(database);
        }

        public async Task<long> DeleteLedgerEntriesByUserIdAsync(string userId)
        {
            var filter = Builders<LedgerEntry>.Filter.Eq(x => x.UserId, userId);

            var result = await _db.GetCollection<LedgerEntry>().DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<long> DeleteRecurringTransactionsByUserIdAsync(string userId)
        {
            var filter = Builders<RecurringTransaction>.Filter.Eq(x => x.UserId, userId);

            var result = await _db.GetCollection<RecurringTransaction>().DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<long> DeleteIncomeGeneratorsByUserIdAsync(string userId)
        {
            var filter = Builders<IncomeGenerator>.Filter.Eq(x => x.UserId, userId);

            var result = await _db.GetCollection<IncomeGenerator>().DeleteManyAsync(filter);
            return result.DeletedCount;
        }

        public async Task<IEnumerable<Frequency>> GetFrequenciesAsync()
        {
            var cursor = await _db.GetCollection<Frequency>().FindAsync(FilterDefinition<Frequency>.Empty);
            return await cursor.ToListAsync();
        }

        public async Task<IEnumerable<RecurringTransaction>> GetRecurringTransactionsByFrequencyAndLastExecutedAsync(string frequencyId, DateTime since)
        {
            var builder = Builders<RecurringTransaction>.Filter;
            var frequencyFilter = builder.Eq(x => x.FrequencyId, frequencyId);
            var lastExecutedFilter = builder.Lte(x => x.LastExecuted, since);

            var cursor = await _db.GetCollection<RecurringTransaction>().FindAsync(frequencyFilter & lastExecutedFilter);
            return await cursor.ToListAsync();
        }

        public async Task InsertLedgerEntryAsync(LedgerEntry entry)
        {
            await _db.GetCollection<LedgerEntry>().InsertOneAsync(entry);
        }

        public async Task UpdateRecurringTransactionLastExecutedAsync(string id, DateTime lastExecuted)
        {
            var filter = Builders<RecurringTransaction>.Filter.Eq(x => x.Id, id);
            var update = Builders<RecurringTransaction>.Update.Set(x => x.LastExecuted, lastExecuted);

            await _db.GetCollection<RecurringTransaction>().UpdateOneAsync(filter, update);
        }

        public async Task UpdateRecurringTransactionLastTriggeredAsync(string id, DateTime lastTriggered)
        {
            var filter = Builders<RecurringTransaction>.Filter.Eq(x => x.Id, id);
            var update = Builders<RecurringTransaction>.Update.Set(x => x.LastTriggered, lastTriggered);

            await _db.GetCollection<RecurringTransaction>().UpdateOneAsync(filter, update);
        }
    }
}