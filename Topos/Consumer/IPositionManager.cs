using System.Threading.Tasks;

namespace Topos.Consumer;

public interface IPositionManager
{
    Task SetAsync(Position position);
    Task<Position> GetAsync(string topic, int partition);
}