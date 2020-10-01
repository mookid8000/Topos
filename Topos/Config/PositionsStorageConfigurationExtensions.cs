using System;
using System.Threading.Tasks;
using Topos.Consumer;

namespace Topos.Config
{
    public static class PositionsStorageConfigurationExtensions
    {
        public static void SetInitialPosition(this StandardConfigurer<IPositionManager> configurer, StartFromPosition startFromPosition)
        {
            var registrar = StandardConfigurer.Open(configurer);

            if (registrar.Other<ExplicitlySetInitialPosition>().HasService())
            {
                throw new InvalidOperationException($"Cannot set resume position to {startFromPosition}, because it has already been set!");
            }

            registrar.Other<ExplicitlySetInitialPosition>().Register(c => new ExplicitlySetInitialPosition(startFromPosition));

            registrar.Decorate(c => new InitialPositionDecorator(c.Get<IPositionManager>(), startFromPosition));
        }

        public class ExplicitlySetInitialPosition
        {
            public StartFromPosition Position { get; }

            public ExplicitlySetInitialPosition(StartFromPosition position) => Position = position;
        }

        class InitialPositionDecorator : IPositionManager
        {
            readonly IPositionManager _positionManager;
            readonly StartFromPosition _startFromPosition;

            public InitialPositionDecorator(IPositionManager positionManager, StartFromPosition startFromPosition)
            {
                _positionManager = positionManager ?? throw new ArgumentNullException(nameof(positionManager));
                _startFromPosition = startFromPosition;
            }

            public Task Set(Position position) => _positionManager.Set(position);

            public async Task<Position> Get(string topic, int partition)
            {
                var position = await _positionManager.Get(topic, partition);

                return (isDefault: position.IsDefault, startFrom: _startFromPosition) switch
                {
                    (isDefault: true, startFrom: StartFromPosition.Beginning) => Position.Default(topic, partition),
                    (isDefault: true, startFrom: StartFromPosition.Now) => Position.OnlyNew(topic, partition),

                    _ => position
                };
            }
        }
    }
}