using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Topos.Producer
{
    public interface IToposProducer : IDisposable
    {
        Task Send(object message, Dictionary<string, string> optionalHeaders = null);
    }
}