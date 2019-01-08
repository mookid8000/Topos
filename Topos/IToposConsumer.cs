using System;

namespace Topos
{
    public interface IToposConsumer : IDisposable
    {
        IDisposable Start();
    }
}