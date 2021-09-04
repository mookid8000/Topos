using System;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace Topos.Config
{
    public class FasterProducerConfigurationBuilder
    {
        readonly Dictionary<string, TimeSpan> _maxAges = new();

        internal IEnumerable<KeyValuePair<string, TimeSpan>> GetMaxAges() => _maxAges;

        /// <summary>
        /// By default, topics are compacted to keep events 7 days back. Call this function to explicitly set the max age for events for the given topic
        /// </summary>
        public FasterProducerConfigurationBuilder SetMaxAge(string topic, TimeSpan maxAge)
        {
            _maxAges[topic] = maxAge;
            return this;
        }
    }
}