using System;
using Topos.Config;

namespace Topos.Routing
{
    /// <summary>
    /// Registers <see cref="SimpleTopicMapper"/>, which is a simple .NET-friendly default topic mapper. Simply uses
    /// short, lower-cased class names as topics.
    /// </summary>
    public static class SimpleTopicMapperConfigurationExtension
    {
        public static void UseSimple(this StandardConfigurer<ITopicMapper> configurer)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            StandardConfigurer.Open(configurer).Register(c => new SimpleTopicMapper());
        }
    }
}