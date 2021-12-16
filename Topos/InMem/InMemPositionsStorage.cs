using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Topos.Consumer;

namespace Topos.InMem;

public class InMemPositionsStorage
{
    readonly ConcurrentDictionary<string, ConcurrentDictionary<int, Position>> _positions = new();

    public void Set(Position position) => GetPositions(position.Topic)[position.Partition] = position;

    public IReadOnlyCollection<Position> Get(string topic, IEnumerable<int> partitions)
    {
        var positions = GetPositions(topic);

        return partitions
            .Select(partition => positions.TryGetValue(partition, out var value) ? value : default)
            .ToList();
    }

    public IReadOnlyCollection<Position> GetAll(string topic) => GetPositions(topic).Values.ToList();

    ConcurrentDictionary<int, Position> GetPositions(string topic) => _positions.GetOrAdd(topic, _ => new ConcurrentDictionary<int, Position>());
}