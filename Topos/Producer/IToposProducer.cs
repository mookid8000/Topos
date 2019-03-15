using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Topos.Producer
{
    public interface IToposProducer : IDisposable
    {
        Task Send(object message, string partitionKey = null, Dictionary<string, string> optionalHeaders = null);
    }
}