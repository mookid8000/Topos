using System;
using Topos.Producer;

namespace Topos.Tests.Contracts
{
    public interface IBrokerFactory : IDisposable
    {
        IToposProducer Create();
    }
}