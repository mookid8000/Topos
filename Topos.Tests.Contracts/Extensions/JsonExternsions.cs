using System;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;

namespace Topos.Tests.Contracts.Extensions
{
    static class JsonExternsions
    {
        public static string ToPrettyJson(this object obj)
        {
            if (obj is IEnumerable enumerable)
            {
                return string.Join(Environment.NewLine, enumerable.Cast<object>().Select(JsonConvert.SerializeObject));
            }

            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
    }
}