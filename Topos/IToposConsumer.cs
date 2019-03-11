using System;

namespace Topos
{
    public interface IToposConsumer : IDisposable
    {
        void Start();
    }
}