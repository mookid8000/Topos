using System.Collections.Generic;
using System.Threading.Tasks;

namespace Topos.Consumer
{
    public interface IPositionManager
    {
        Task Set(Position position);
        Task<IReadOnlyCollection<Position>> Get(string topic, IEnumerable<int> partitions);
        Task<IReadOnlyCollection<Position>> GetAll(string topic);
    }
}