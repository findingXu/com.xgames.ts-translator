using System;
using System.Reflection;
using System.Text;

namespace XGames.TsTranslator
{
    public static class TsEnum
    {
        public static void Convert(Type dotType, in StringBuilder sb)
        {
            var enumName = dotType.Name;
            sb.AppendLine($"enum {enumName} {{");
            var fieldInfos = dotType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fieldInfos)
            {
                var memName = field.Name;
                var memValue = field.GetRawConstantValue().ToString();
                var enumField = $"{memName} = {memValue},";
                sb.AppendLine(enumField);
            }            
            sb.AppendLine("}");
        }
    }
}