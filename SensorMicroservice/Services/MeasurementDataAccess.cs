using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SensorMicroservice.Models;
using System.Collections;

namespace SensorMicroservice.Services
{
    public class MeasurementDataAccess
    {
        private readonly IMongoCollection<Measurement> _measurementsCollection;

        public MeasurementDataAccess(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var _database = client.GetDatabase(databaseName);
            _measurementsCollection = _database.GetCollection<Measurement>(collectionName);
        }

        public async Task<List<Measurement>> GetAsync() =>
            await _measurementsCollection.Find(_ => true).Limit(15).ToListAsync();

        public async Task<Measurement?> GetAsync(int id) =>
            await _measurementsCollection.Find(x => x.UID == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Measurement newMeasurment) =>
            await _measurementsCollection.InsertOneAsync(newMeasurment);

        public async Task UpdateAsync(int id, Measurement updatedMeasurement) =>
            await _measurementsCollection.ReplaceOneAsync(x => x.UID == id, updatedMeasurement);

        public async Task RemoveAsync(int id) =>
            await _measurementsCollection.DeleteOneAsync(x => x.UID == id);

        public async Task<IAsyncCursor<Measurement>> GetCursorAsync()
        {
            return await _measurementsCollection.FindAsync(FilterDefinition<Measurement>.Empty);
        }
    }
}
 