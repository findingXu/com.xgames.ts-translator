using System.Reflection;

namespace XGames.TsTranslator
{
    public static class TsField
    {
        public static string Convert(FieldInfo fieldInfo)
        {
            var fieldTsType = TsType.Convert(fieldInfo.FieldType);
            var fieldName = fieldInfo.Name;
            var staticKey = fieldInfo.IsStatic ? "static " : "";
            var fieldExpression = $"{staticKey}{fieldName}: {fieldTsType};";
            return fieldExpression;
        }
    }
}