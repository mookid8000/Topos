using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace Topos.Internals
{
    /// <summary>
    /// Hack until C#7 works as it should when targeting .NET Standard
    /// </summary>
    static class KvpExtensions
    {
        public static void Deconstruct<TKey, TValue>(
            this KeyValuePair<TKey, TValue> kvp,
            out TKey key,
            out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}