using System;
using NUnit.Framework;
using Topos.Consumer;
using Topos.Tests.Contracts;
using Topos.Tests.Contracts.Factories;
using Topos.Tests.Contracts.Positions;

namespace Topos.MongoDb.Tests
{
    [TestFixture]
    public class MongoDbPositionManagerTest : PositionsManagerTest<MongoDbPositionManagerTest.MongoDbPositionManagerFactory>
    {
        public class MongoDbPositionManagerFactory : IPositionsManagerFactory
        {
            public IPositionManager Create()
            {
                var collectionName = Guid.NewGuid().ToString("N");
                var database = MongoTestConfig.GetCleanTestDatabase();
                return new MongoDbPositionManager(database, collectionName);
            }

            public void Dispose()
            {
            }
        }
    }
}