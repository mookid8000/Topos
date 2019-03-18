using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Topos.Consumer;
#pragma warning disable 1998

namespace Topos.InMem
{
    public class InMemPositionsManager : IPositionManager
    {
        readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Position>> _positions = new ConcurrentDictionary<string, ConcurrentDictionary<int, Position>>();

        public async Task Set(Position position) => GetPositions(position.Topic)[position.Partition] = position;

        public async Task<IReadOnlyCollection<Position>> Get(string topic, IEnumerable<int> partitions)
        {
            var positions = GetPositions(topic);
            return partitions.Select(p => positions.GetOrAdd(p, _ => new Position(topic, p, -1))).ToList();
        }

        public async Task<IReadOnlyCollection<Position>> GetAll(string topic) => GetPositions(topic).Values.ToList();

        ConcurrentDictionary<int, Position> GetPositions(string topic)
        {
            return _positions
                .GetOrAdd(topic, _ => new ConcurrentDictionary<int, Position>());
        }
    }
}