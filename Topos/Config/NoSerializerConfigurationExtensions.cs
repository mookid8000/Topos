using System;
using Topos.Serialization;

namespace Topos.Config
{
    public static class NoSerializerConfigurationExtensions
    {
        /// <summary>
        /// Configures Topos to skip serialization. This means that the only valid payload to send is <code>byte[]</code>
        /// and received messages will simply be passed to the handler as the <code>byte[]</code> they are.
        /// </summary>
        public static void None(this StandardConfigurer<IMessageSerializer> configurer)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            StandardConfigurer.Open(configurer).Register(c => new RawMessageSerializer());
        }
    }
}