using System.Collections.Generic;
using System.Threading.Tasks;

namespace Topos
{
    public interface IToposProducer
    {
        Task Send(object message, IDictionary<string, string> optionalHeaders = null);
    }
}