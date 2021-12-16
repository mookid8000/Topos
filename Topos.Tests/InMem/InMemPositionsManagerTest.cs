using NUnit.Framework;
using Topos.Consumer;
using Topos.InMem;
using Topos.Tests.Contracts;
using Topos.Tests.Contracts.Factories;
using Topos.Tests.Contracts.Positions;
// ReSharper disable CoVariantArrayConversion

namespace Topos.Tests.InMem;

[TestFixture]
public class InMemPositionsManagerTest : PositionsManagerTest<InMemPositionsManagerTest.InMemPositionManagerFactory>
{
    public class InMemPositionManagerFactory : IPositionsManagerFactory
    {
        public IPositionManager Create() => new InMemPositionsManager(new InMemPositionsStorage());

        public void Dispose()
        {
        }
    }
}