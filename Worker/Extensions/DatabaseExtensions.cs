using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Worker
{
    public static class DatabaseExtensions
    {
        public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase database) => database.GetCollection<T>(typeof(T).GetClassName());

        private static string GetClassName(this Type type) => type.ToString().Split('.').Last();

        public static async Task<IEnumerable<T>> FindWithFilterAsync<T>(this IMongoDatabase database, FilterDefinition<T> filter)
        {
            var cursor = await database.GetCollection<T>().FindAsync(filter);
            return await cursor.ToListAsync();
        }

        public static async Task<long> DeleteWithFilterAsync<T>(this IMongoDatabase database, FilterDefinition<T> filter)
        {
            var result = await database.GetCollection<T>().DeleteManyAsync(filter);
            return result.DeletedCount;
        }
    }
}