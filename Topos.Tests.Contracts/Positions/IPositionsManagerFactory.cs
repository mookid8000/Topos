using System;
using Topos.Consumer;

namespace Topos.Tests.Contracts.Positions
{
    public interface IPositionsManagerFactory : IDisposable
    {
        IPositionManager Create();
    }
}