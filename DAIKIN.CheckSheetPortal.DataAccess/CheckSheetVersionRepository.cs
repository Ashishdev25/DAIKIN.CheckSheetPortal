using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure.DataAccess;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace DAIKIN.CheckSheetPortal.DataAccess
{
    public class CheckSheetVersionRepository : Repository<CheckSheetVersion>, ICheckSheetVersionRepository
    {
        private readonly IMongoCollection<CheckSheetVersion> _collection;
        public CheckSheetVersionRepository(string connectionString, string databaseName, string collectionName) : base(connectionString, databaseName, collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<CheckSheetVersion>(collectionName);
        }
        public async Task<IEnumerable<CheckSheetVersion>> GetLatestVersionsAsync()
        {
            var pipeline = new List<BsonDocument>
            {
                BsonDocument.Parse("{ $sort: { UniqueId: 1, StationId: 1, LineId: 1, Version: -1 } }"), // Sort by UniqueId, StationId, LineId, and Version in descending order
                BsonDocument.Parse("{ $group: { _id: { UniqueId: '$UniqueId', StationId: '$StationId', LineId: '$LineId' }, latestVersion: { $first: '$$ROOT' } } }"), // Group by UniqueId, StationId, and LineId and take the first document in each group
                BsonDocument.Parse("{ $replaceRoot: { newRoot: '$latestVersion' } }") // Replace the root document with the latestVersion document
            };

            var result = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            var latestVersions = result.Select(document => BsonSerializer.Deserialize<CheckSheetVersion>(document));

            return latestVersions.ToList();
        }

    }
}
