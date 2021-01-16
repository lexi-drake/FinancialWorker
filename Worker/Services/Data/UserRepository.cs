using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Worker
{
    public class UserRepository : IUserRespository
    {
        private IMongoDatabase _db;

        public UserRepository(string connectionString, string database)
        {
            var client = new MongoClient(connectionString);
            _db = client.GetDatabase(database);
        }

        public async Task<IEnumerable<User>> GetUsersByLastLoggedInAsync(DateTime since)
        {
            var filter = Builders<User>.Filter.Lte(x => x.LastLoggedIn, since);

            var cursor = await _db.GetCollection<User>().FindAsync(filter);
            return await cursor.ToListAsync();
        }

        public async Task DeleteUserByIdAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Id, id);
            await _db.GetCollection<User>().DeleteOneAsync(filter);
        }
    }
}