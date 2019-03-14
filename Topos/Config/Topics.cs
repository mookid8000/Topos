using System;
using System.Collections;
using System.Collections.Generic;

namespace Topos.Config
{
    public class Topics : IEnumerable<string>
    {
        readonly HashSet<string> _topics = new HashSet<string>();

        public void AddRange(IEnumerable<string> topics)
        {
            if (topics == null) throw new ArgumentNullException(nameof(topics));
            foreach (var topic in topics)
            {
                _topics.Add(topic);
            }
        }

        public IEnumerator<string> GetEnumerator() => _topics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => string.Join(", ", _topics);
    }
}