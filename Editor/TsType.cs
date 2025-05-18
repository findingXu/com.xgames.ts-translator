using System;
using System.Collections.Generic;
using System.Linq;

namespace XGames.TsTranslator
{
    public static class TsType
    {
        public const string Any = "any";
        public const string T = "T";
        public const string Void = "void";
        public const string Array = "Array";
        public const string LuaTable = "LuaTable";
        public const string Boolean = "boolean";
        public const string Number = "number";
        public const string String = "string";

        public static string Convert(Type dotType)
        {
            var elementType = dotType.GetElementType() ?? dotType;
            if (elementType.IsNested) return Any;
            if (dotType.IsGenericParameter) return T;
            if (ConvertValueType(dotType, out var valueType)) return valueType;
            if (dotType.IsGenericType) return GetGenericTypeName(dotType);
            if (!TypeTranslator.LuaCallCSharp.Contains(elementType)) return Any;

            var targetType = dotType.IsByRef || dotType.IsPointer ? elementType : dotType;
            return targetType.FullName ?? targetType.Name;
        }

        private static string GetGenericTypeName(Type dotType)
        {
            var elementType = dotType.GetElementType() ?? dotType;
            var typeName = elementType.FullName ?? elementType.Name;

            var genericType = dotType.GetGenericTypeDefinition();
            if (genericType == typeof(List<>) || genericType == typeof(IList<>) || genericType == typeof(ICollection<>) || genericType == typeof(IEnumerable<>))
            {
                typeName = Array;
            }
            else if (genericType == typeof(Dictionary<,>) || genericType == typeof(IDictionary<,>))
            {
                typeName = LuaTable;
            }
            else if (!TypeTranslator.LuaCallCSharp.Contains(elementType))
            {
                return Any;
            }

            var genericArguments = dotType.GetGenericArguments().Select(g =>
            {
                var genType = Convert(g);
                return genType;
            });

            typeName = $"{typeName}<{string.Join(", ", genericArguments)}>";
            return typeName;
        }

        private static bool ConvertValueType(Type type, out string result)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(bool))
                {
                    result = Boolean;
                }
                else if (type == typeof(char))
                {
                    result = String;
                }
                else
                {
                    result = Number;
                }

                return true;
            }

            result = Any;
            if (type == typeof(void))
            {
                result = Void;
            }
            else if (type == typeof(string))
            {
                result = String;
            }

            return result != Any;
        }
    }
}