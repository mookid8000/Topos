using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Topos.Consumer;

namespace Topos.MongoDb
{
    public class MongoDbPositionManager : IPositionManager
    {
        readonly IMongoCollection<BsonDocument> _positions;
        static readonly Position[] EmptyListOfPositions = Enumerable.Empty<Position>().ToArray();

        public MongoDbPositionManager(IMongoDatabase database, string collectionName)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (collectionName == null) throw new ArgumentNullException(nameof(collectionName));
            _positions = database.GetCollection<BsonDocument>(collectionName);
        }

        public async Task Set(Position position)
        {
            var criteria = new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument
            {
                {"_id", position.Topic}
            });

            var update = new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument
            {
                {"$set", new BsonDocument{{position.Partition.ToString(), position.Offset}}}
            });

            await _positions.UpdateOneAsync(criteria, update, new UpdateOptions { IsUpsert = true });
        }

        public async Task<IReadOnlyCollection<Position>> Get(string topic, IEnumerable<int> partitions)
        {
            var array = partitions.ToArray();
            var allPositions = await GetAll(topic);
            return allPositions.Where(p => array.Contains(p.Partition)).ToArray();
        }

        public async Task<IReadOnlyCollection<Position>> GetAll(string topic)
        {
            var query = new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument
            {
                {"_id", topic}
            });

            var document = await _positions.Find(query).FirstOrDefaultAsync();
            if (document == null) return EmptyListOfPositions;

            return document
                .Where(element => element.Name != "_id")
                .Select(element =>
                {
                    if (!int.TryParse(element.Name, out var partition))
                    {
                        throw new ArgumentException($"Could not parse partition '{element.Name}' into an integer partition number!");
                    }
                    return new Position(topic, partition, element.Value.AsInt64);
                })
                .ToArray();
        }
    }
}