using System;
using System.Collections.Generic;

namespace Topos.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> Clone(this Dictionary<string, string> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            return new Dictionary<string, string>(dictionary);
        }
    }
}