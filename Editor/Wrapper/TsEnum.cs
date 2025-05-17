using System;
using System.Linq;
using System.Reflection;

namespace XGames.TsTranslator
{
    public static class TsEnum
    {
        public static string Convert(Type dotType)
        {
            var enumName = dotType.Name;
            var enumFields = ConvertFields(dotType);
            var fieldExpressions = TsTypePrinter.TabScope(string.Join(",\n", enumFields));
            var tsEnum = $"enum {enumName} {{ {fieldExpressions} }}";
            return tsEnum;
        }

        private static string[] ConvertFields(Type dotType)
        {
            var fieldInfos = dotType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var fields = fieldInfos.Select(field =>
            {
                var memName = field.Name;
                var memValue = field.GetRawConstantValue().ToString();
                var enumField = $"{memName} = {memValue}";
                return enumField;
            }).ToArray();

            return fields;
        }
    }
}