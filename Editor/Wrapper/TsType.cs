using System;
using System.Collections.Generic;
using System.Linq;

namespace XGames.TsTranslator
{
    public static class TsType
    {
        public const string ANY = "any";

        public static string Convert(Type dotType)
        {
            if (dotType.IsGenericParameter)
            {
                return "T";
            }

            if (dotType.IsPointer || dotType.IsByRef || dotType.IsNested)
            {
                return "any";
            }

            if (dotType.IsArray)
            {
                var elementType = dotType.GetElementType();
                if (elementType != null && elementType.IsNested) return "any";
            }

            if (ConvertValueType(dotType, out var valueTypeName))
            {
                return valueTypeName;
            }

            if (dotType.IsGenericType)
            {
                return GetGenericTypeName(dotType);
            }

            if (!TsTypePrinter.LuaCallCSharp.Contains(dotType))
            {
                return "any";
            }

            return dotType.FullName ?? dotType.Name;
        }

        private static string GetGenericTypeName(Type type)
        {
            var typeName = type.FullName ?? type.Name;

            var genericType = type.GetGenericTypeDefinition();
            if (genericType == typeof(List<>)
                || genericType == typeof(IList<>)
                || genericType == typeof(ICollection<>)
                || genericType == typeof(IEnumerable<>))
            {
                typeName = "Array";
            }
            else if (genericType == typeof(Dictionary<,>)
                     || genericType == typeof(IDictionary<,>))
            {
                typeName = "LuaTable";
            }
            else
            {
                if (!TsTypePrinter.LuaCallCSharp.Contains(type))
                {
                    return "any";
                }

                typeName = typeName[..typeName.IndexOf('`')];
            }

            var genericArguments = type.GetGenericArguments().Select(g =>
            {
                var genType = TsType.Convert(g);
                return genType;
            });

            var tsType = $"{typeName}<{string.Join(", ", genericArguments)}>";

            if (type.IsArray)
            {
                var rank = type.GetArrayRank();
                for (int i = 0; i < rank; ++i)
                {
                    tsType += "[]";
                }
            }

            return tsType;
        }

        private static bool ConvertValueType(Type type, out string result)
        {
            result = "any";
            if (type == typeof(void))
            {
                result = "void";
            }

            if (type == typeof(bool))
            {
                result = "boolean";
            }

            if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
                || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong)
                || type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                result = "number";
            }

            if (type == typeof(char) || type == typeof(string))
            {
                result = "string";
            }

            if (type == typeof(bool))
            {
                result = "boolean";
            }

            return result != "any";
        }
    }
}