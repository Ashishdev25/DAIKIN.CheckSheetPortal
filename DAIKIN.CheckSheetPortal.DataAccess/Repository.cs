using DAIKIN.CheckSheetPortal.Entities;
using DAIKIN.CheckSheetPortal.Infrastructure;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace DAIKIN.CheckSheetPortal.DataAccess
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;
        private string _connectionString;
        private string _databaseName;
        public Repository(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<T>(collectionName);
            _connectionString = connectionString;
            _databaseName = databaseName;
            RegisterClassMap();
        }
        private void RegisterClassMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var cursor = await _collection.FindAsync(_ => true);
            return await cursor.ToListAsync();
        }
        public async Task<(IEnumerable<T>, long)> GetAllWithPaginationAsync(DynamicTable dynamicTable)
        {
            var query = Builders<T>.Filter.Empty;

            if (!string.IsNullOrEmpty(dynamicTable.GlobalSearch))
            {
                var modelName = typeof(T).Name;
                var searchTerms = dynamicTable.GlobalSearch.Split(',');
                if (modelName == "CheckSheetTransaction")
                {
                    var filters = searchTerms.Select(term => term.Trim()).Select(trimmedTerm =>
                                    Builders<T>.Filter.Regex("Name", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("ChangeDetails", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("Revision", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("Department", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("Line", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("MaintenaceClass", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("Station", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("Equipment", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("EquipmentCode", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("Location", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("SubLocation", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.Regex("Status", new BsonRegularExpression(trimmedTerm, "i")) |
                                    Builders<T>.Filter.ElemMatch("CheckPointTransactions", Builders<CheckPointTransaction>.Filter.Regex("CheckRecord", new BsonRegularExpression(trimmedTerm, "i"))));
                    query = Builders<T>.Filter.And(filters);
                    query &= Builders<T>.Filter.Eq("CheckSheetDay", DateTime.UtcNow.Date + TimeSpan.Zero);
                }
                else if(modelName == "CheckSheetVersion")
                {
                    if (dynamicTable.IncludeVersion)
                    {
                        var filters = searchTerms.Select(term => term.Trim()).Select(trimmedTerm =>
                                        Builders<T>.Filter.Regex("Name", new BsonRegularExpression(trimmedTerm, "i")) |
                                        Builders<T>.Filter.Regex("ChangeDetails", new BsonRegularExpression(trimmedTerm, "i")) |
                                        Builders<T>.Filter.Regex("Revision", new BsonRegularExpression(trimmedTerm, "i")) |
                                        Builders<T>.Filter.Regex("Status", new BsonRegularExpression(trimmedTerm, "i")));
                        query = Builders<T>.Filter.And(filters);
                    }
                    else
                    {
                        var pipeline = new List<BsonDocument>
                        {
                            BsonDocument.Parse("{ $sort: { UniqueId: 1, StationId: 1, LineId: 1, Version: -1 } }"), // Sort by UniqueId, StationId, LineId, and Version in descending order
                            BsonDocument.Parse("{ $group: { _id: { UniqueId: '$UniqueId', StationId: '$StationId', LineId: '$LineId' }, latestVersion: { $first: '$$ROOT' } } }"), // Group by UniqueId, StationId, and LineId and take the first document in each group
                            BsonDocument.Parse("{ $replaceRoot: { newRoot: '$latestVersion' } }") // Replace the root document with the latestVersion document
                        };
                        var result = await _collection.Aggregate<BsonDocument>(pipeline)
                                                      .ToListAsync();

                        var latestVersions = result.Select(document => BsonSerializer.Deserialize<T>(document));

                        return (latestVersions, latestVersions.Count());
                    }
                }
            }
            else if (dynamicTable.DynamicFilter != null && dynamicTable.DynamicFilter.Count > 0)
            {
                foreach (var dynamicFilter in dynamicTable.DynamicFilter)
                {
                    var condition = dynamicFilter.Condition.ToLower();
                    switch (condition)
                    {
                        case "equal":
                            query &= Builders<T>.Filter.Eq(dynamicFilter.FieldName, dynamicFilter.FieldValue);
                            break;
                        case "contains":
                            query &= Builders<T>.Filter.Regex(dynamicFilter.FieldName, new BsonRegularExpression($".*{dynamicFilter.FieldValue}.*", "i"));
                            break;
                        case "starts with":
                            query &= Builders<T>.Filter.Regex(dynamicFilter.FieldName, new BsonRegularExpression($"^{dynamicFilter.FieldValue}.*", "i"));
                            break;
                        default:
                            query &= Builders<T>.Filter.Eq(dynamicFilter.FieldName, dynamicFilter.FieldValue);
                            break;
                    }
                }
            }

            var sort = Builders<T>.Sort;
            var sortDefinition = dynamicTable.SortDirection == "asc"
                ? sort.Ascending(dynamicTable.SortField)
                : sort.Descending(dynamicTable.SortField);

            var documents = await _collection.Find(query)
                .Sort(sortDefinition)
                .Skip((dynamicTable.PageNumber - 1) * dynamicTable.PageSize)
                .Limit(dynamicTable.PageSize)
                .ToListAsync();

            var recordCount = await _collection.CountDocumentsAsync(query);

            return (documents, recordCount);
        }
        public async Task<T> GetByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            var cursor = await _collection.FindAsync(filter);
            return await cursor.FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<T>> GetByFilterAsync(string column, object value)
        {
            var filter = Builders<T>.Filter.Eq(column, value);
            var cursor = await _collection.FindAsync(filter);
            return await cursor.ToListAsync();
        }
        public async Task<IEnumerable<T>> GetByBetweenDatesAsync(string column, DateTime fromDate, DateTime toDate)
        {
            var filter = Builders<T>.Filter.And(
                Builders<T>.Filter.Gte(column, fromDate),
                Builders<T>.Filter.Lte(column, toDate)
            );

            var cursor = await _collection.FindAsync(filter);
            return await cursor.ToListAsync();
        }
        public async Task<IEnumerable<T>> GetByFilterAsync(Dictionary<string, object> filters)
        {
            var filterBuilder = Builders<T>.Filter;
            var conditions = new List<FilterDefinition<T>>();

            foreach (var kvp in filters)
            {
                var condition = filterBuilder.Eq(kvp.Key, kvp.Value);
                conditions.Add(condition);
            }

            var filter = filterBuilder.And(conditions);
            var cursor = await _collection.FindAsync(filter);
            return await cursor.ToListAsync();
        }
        public async Task<bool> GetByFilterAnyAsync(string column, object value)
        {
            var filter = Builders<T>.Filter.Eq(column, value);
            var count = await _collection.CountDocumentsAsync(filter);
            return count > 0;
        }
        public async Task<T> FirstOrDefaultAsync(string column, object value)
        {
            var filter = Builders<T>.Filter.Eq(column, value);
            var cursor = await _collection.FindAsync(filter);
            return await cursor.FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<T>> GetByFilterInAsync(string column, IEnumerable<object> values)
        {
            var filter = Builders<T>.Filter.In(column, values);
            var cursor = await _collection.FindAsync(filter);
            return await cursor.ToListAsync();
        }
        public async Task<T> CreateAsync(T document)
        {
            await _collection.InsertOneAsync(document);
            return document;
        }
        public async Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> documents)
        {
            await _collection.InsertManyAsync(documents);
            return documents;
        }
        public async Task InsertBulkAsync(IEnumerable<T> documents)
        {
            await _collection.InsertManyAsync(documents);
        }
        public void InsertBulk(IEnumerable<T> documents)
        {
            _collection.InsertMany(documents);
        }
        public async Task UpdateAsync(string id, T document)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            await _collection.ReplaceOneAsync(filter, document);
        }
        public async Task DeleteAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            await _collection.DeleteOneAsync(filter);
        }
        public async Task DeleteManyAsync(string column, object value)
        {
            var filter = Builders<T>.Filter.Eq(column, value);
            await _collection.DeleteManyAsync(filter);
        }
        public async Task DeleteManyValuesAsync(string column, List<string> values)
        {
            var filter = Builders<T>.Filter.In(column, values);
            await _collection.DeleteManyAsync(filter);
        }
        public async Task DropTableAsync(string tablename)
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase(_databaseName);
            await database.DropCollectionAsync(tablename);
        }
        public async Task<int> GetRecordCountAsync()
        {
            var count = _collection.AsQueryable().Count();
            return count;
        }
    }
}