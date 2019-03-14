using System;

namespace Topos
{
    public interface IToposConsumerImplementation : IDisposable
    {
        void Start();
    }
}