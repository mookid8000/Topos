using System;
using System.Collections.Generic;
using System.Linq;

namespace Topos.Internals
{
    class ConnectionStringParser
    {
        readonly List<KeyValuePair<string, string>> _values;

        public ConnectionStringParser(string connectionString)
        {
            _values = connectionString.Split(';')
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part.Split('='))
                .Select(parts => new KeyValuePair<string, string>(parts.First(), string.Join("=", parts.Skip(1))))
                .ToList();
        }

        public bool HasElement(string name) => _values.Any(v => string.Equals(v.Key, name, StringComparison.OrdinalIgnoreCase));

        public string GetValue(string name)
        {
            var match = _values.FirstOrDefault(v => string.Equals(v.Key, name, StringComparison.OrdinalIgnoreCase));
            
            return match.Value;
        }
    }
}