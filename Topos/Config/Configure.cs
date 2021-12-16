using System;

namespace Topos.Config;

public static class Configure
{
    /// <summary>
    /// Configures a Topos producer.
    /// </summary>
    /// <param name="configure">Configuration callback builder</param>
    public static ToposProducerConfigurer Producer(Action<StandardConfigurer<IProducerImplementation>> configure) => new ToposProducerConfigurer(configure);
        
    /// <summary>
    /// Configures a Topos consumer.
    /// </summary>
    /// <param name="groupName">
    /// Defines the name of the consumer group. Groups with the same name will compete for messages, meaning
    /// that available partitions will be divided among instances within each consumer group name.
    /// </param>
    /// <param name="configure">Configuration callback builder</param>
    public static ToposConsumerConfigurer Consumer(string groupName, Action<StandardConfigurer<IConsumerImplementation>> configure) => new ToposConsumerConfigurer(configure, groupName);
}