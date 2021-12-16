using System;
using System.Linq;
using System.Threading.Tasks;
using Topos.Consumer;
#pragma warning disable 1998

namespace Topos.InMem;

public class InMemPositionsManager : IPositionManager
{
    readonly InMemPositionsStorage _positionsStorage;

    public InMemPositionsManager(InMemPositionsStorage positionsStorage)
    {
        _positionsStorage = positionsStorage ?? throw new ArgumentNullException(nameof(positionsStorage));
    }

    public async Task Set(Position position) => _positionsStorage.Set(position);

    public async Task<Position> Get(string topic, int partition)
    {
        var results = _positionsStorage.Get(topic, new[] { partition });

        return results.FirstOrDefault() ?? Position.Default(topic, partition);
    }
}