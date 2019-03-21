using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Topos.Consumer;
#pragma warning disable 1998

namespace Topos.InMem
{
    public class InMemPositionsManager : IPositionManager
    {
        readonly InMemPositionsStorage _positionsStorage;

        public InMemPositionsManager(InMemPositionsStorage positionsStorage)
        {
            _positionsStorage = positionsStorage ?? throw new ArgumentNullException(nameof(positionsStorage));
        }

        public async Task Set(Position position) => _positionsStorage.Set(position);

        public async Task<IReadOnlyCollection<Position>> Get(string topic, IEnumerable<int> partitions) => _positionsStorage.Get(topic, partitions);

        public async Task<IReadOnlyCollection<Position>> GetAll(string topic) => _positionsStorage.GetAll(topic);
    }
}