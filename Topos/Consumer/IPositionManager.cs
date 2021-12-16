using System.Threading.Tasks;

namespace Topos.Consumer;

public interface IPositionManager
{
    Task Set(Position position);
    Task<Position> Get(string topic, int partition);
}