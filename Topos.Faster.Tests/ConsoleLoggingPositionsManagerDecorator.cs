using System;
using System.Threading.Tasks;
using Topos.Consumer;

namespace Topos.Faster.Tests;

#pragma warning disable CS1998
class ConsoleLoggingPositionsManagerDecorator : IPositionManager
{
    readonly IPositionManager _positionManager;

    public ConsoleLoggingPositionsManagerDecorator(IPositionManager positionManager) => _positionManager = positionManager ?? throw new ArgumentNullException(nameof(positionManager));

    public async Task Set(Position position)
    {
        Console.WriteLine($"Setting position {position}");
        await _positionManager.Set(position);
    }

    public async Task<Position> Get(string topic, int partition)
    {
        var position = await _positionManager.Get(topic, partition);
        Console.WriteLine($"Got position {position}");
        return position;
    }
}