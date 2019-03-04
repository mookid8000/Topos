using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Topos
{
    public interface IToposProducer : IDisposable
    {
        Task Send(object message, IDictionary<string, string> optionalHeaders = null);
    }
}