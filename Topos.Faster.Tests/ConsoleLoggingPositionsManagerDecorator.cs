using System;
using System.Threading.Tasks;
using Topos.Consumer;

namespace Topos.Faster.Tests;

#pragma warning disable CS1998
class ConsoleLoggingPositionsManagerDecorator(IPositionManager positionManager) : IPositionManager
{
    public async Task SetAsync(Position position)
    {
        Console.WriteLine($"Setting position {position}");
        await positionManager.SetAsync(position);
    }

    public async Task<Position> GetAsync(string topic, int partition)
    {
        var position = await positionManager.GetAsync(topic, partition);
        Console.WriteLine($"Got position {position}");
        return position;
    }
}