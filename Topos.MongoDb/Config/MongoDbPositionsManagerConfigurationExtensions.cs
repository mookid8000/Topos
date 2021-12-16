using System;
using MongoDB.Driver;
using Topos.Consumer;
using Topos.MongoDb;
// ReSharper disable UnusedMember.Global

namespace Topos.Config;

public static class MongoDbPositionsManagerConfigurationExtensions
{
    /// <summary>
    /// Configures Topos to stores its consumer positions in documents in MongoDB. Individual documents will be created for each relevant topic with fields for each partition.
    /// </summary>
    public static void StoreInMongoDb(this StandardConfigurer<IPositionManager> configurer, IMongoDatabase database, string collectionName)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        if (database == null) throw new ArgumentNullException(nameof(database));
        if (collectionName == null) throw new ArgumentNullException(nameof(collectionName));

        var registrar = StandardConfigurer.Open(configurer);

        registrar.Register(_ => new MongoDbPositionManager(database, collectionName));
    }
}