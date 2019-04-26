using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Topos.Internals
{
    static class StreamReaderExtensions
    {
        static readonly FieldInfo CharPosField;
        static readonly FieldInfo CharLenField;

        static StreamReaderExtensions()
        {
            const BindingFlags bindingFlags = BindingFlags.DeclaredOnly
                                              | BindingFlags.Public
                                              | BindingFlags.NonPublic
                                              | BindingFlags.Instance
                                              | BindingFlags.GetField;

            var type = typeof(StreamReader);

            FieldInfo GetField(params string[] fieldNames)
            {
                var fields = fieldNames
                    .Select(fieldName => type.GetField(fieldName, bindingFlags));

                var match = fields.FirstOrDefault(f => f != null);

                return match ?? throw new ArgumentException(
                           $@"Could not find field named either of {string.Join(", ", fieldNames)} from the type {type} - found the following fields:

{string.Join(Environment.NewLine, type.GetFields(bindingFlags).Select(field => $"    {field.Name}"))}");
            }

            CharPosField = GetField("charPos", "_charPos");
            CharLenField = GetField("charLen", "_charLen");
        }

        public static int GetBytePosition(this StreamReader reader)
        {
            var charpos = (int)CharPosField.GetValue(reader);
            var charlen = (int)CharLenField.GetValue(reader);

            return (int)reader.BaseStream.Position - charlen + charpos;
        }
    }
}