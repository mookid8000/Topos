using System;

namespace Topos.Consumer
{
    public interface IToposConsumer : IDisposable
    {
        void Start();
    }
}