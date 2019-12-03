using System;
using System.Collections.Concurrent;
using System.Text;

namespace Topos.Extensions
{
    public static class TypeExtensions
    {
        static readonly ConcurrentDictionary<Type, string> TypeNameCache = new ConcurrentDictionary<Type, string>();
        static readonly ConcurrentDictionary<string, Type> TypeCache = new ConcurrentDictionary<string, Type>();

        public static string GetSimpleAssemblyQualifiedTypeName(this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return TypeNameCache.GetOrAdd(type,_ => BuildSimpleAssemblyQualifiedName(type, new StringBuilder()).ToString());
        }

        public static Type ParseType(this string simpleAssemblyQualifiedTypeName)
        {
            if (simpleAssemblyQualifiedTypeName == null) throw new ArgumentNullException(nameof(simpleAssemblyQualifiedTypeName));

            return TypeCache.GetOrAdd(simpleAssemblyQualifiedTypeName, _ => InnerParseType(simpleAssemblyQualifiedTypeName));
        }

        static StringBuilder BuildSimpleAssemblyQualifiedName(Type type, StringBuilder sb)
        {
            var assemblyName = GetAssemblyName(type);

            if (!type.IsGenericType)
            {
                sb.Append($"{type.FullName}, {assemblyName}");
                return sb;
            }

            if (!type.IsConstructedGenericType)
            {
                return sb;
            }

            var fullName = type.FullName ?? "???";
            var requiredPosition = fullName.IndexOf("[", StringComparison.Ordinal);
            var name = fullName.Substring(0, requiredPosition);
            sb.Append($"{name}[");

            var arguments = type.GetGenericArguments();
            for (var i = 0; i < arguments.Length; i++)
            {
                sb.Append(i == 0 ? "[" : ", [");
                BuildSimpleAssemblyQualifiedName(arguments[i], sb);
                sb.Append("]");
            }

            sb.Append($"], {assemblyName}");

            return sb;
        }

        static string GetAssemblyName(Type type)
        {
            var assemblyName = type.Assembly.GetName().Name;

            return assemblyName == "System.Private.CoreLib"
                ? "mscorlib"
                : assemblyName;
        }

        static Type InnerParseType(string typeName)
        {
            try
            {
                var type = Type.GetType(typeName);

                return type ?? throw new ArgumentException("Type not found");
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Could not find .NET type named '{typeName}'", exception);
            }
        }
    }
}