using System;
using Topos.Consumer;

namespace Topos.Tests.Contracts.Factories
{
    public interface IPositionsManagerFactory : IDisposable
    {
        IPositionManager Create();
    }
}