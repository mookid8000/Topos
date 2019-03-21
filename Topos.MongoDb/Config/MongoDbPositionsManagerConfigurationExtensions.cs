using System;
using MongoDB.Driver;
using Topos.Consumer;
using Topos.MongoDb;

namespace Topos.Config
{
    public static class MongoDbPositionsManagerConfigurationExtensions
    {
        public static void StoreInMongoDb(this StandardConfigurer<IPositionManager> configurer, IMongoDatabase database, string collectionName)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (collectionName == null) throw new ArgumentNullException(nameof(collectionName));

            var registrar = StandardConfigurer.Open(configurer);

            registrar.Register(c => new MongoDbPositionManager(database, collectionName));
        }
    }
}