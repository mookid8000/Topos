using System.Threading.Tasks;

namespace Topos
{
    public interface IToposProducer
    {
        Task Send(object message);
    }
}