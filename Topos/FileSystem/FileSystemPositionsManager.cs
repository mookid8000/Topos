using System.Threading.Tasks;
using Topos.Consumer;

namespace Topos.FileSystem
{
    public class FileSystemPositionsManager : IPositionManager
    {
        public Task Set(Position position)
        {
            throw new System.NotImplementedException();
        }

        public Task<Position?> Get(string topic, int partition)
        {
            throw new System.NotImplementedException();
        }
    }
}