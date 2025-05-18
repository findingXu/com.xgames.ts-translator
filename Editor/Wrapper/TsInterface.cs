using System;
using System.Linq;
using System.Reflection;

namespace XGames.TsTranslator
{
    public static class TsInterface
    {
        public static string Convert(Type dotType)
        {
            var interfaceName = dotType.Name;

            var extendsName = string.Empty;
            if (dotType.BaseType is { IsValueType: false })
            {
                extendsName = TsType.Convert(dotType.BaseType);
            }

            var extendsExpression = !string.IsNullOrEmpty(extendsName) && extendsName != "any"
                ? $" extends {extendsName}"
                : "";

            var fields = ConvertFields(dotType);
            var methods = ConvertMethods(dotType);
            var tsContent = fields.Concat(methods);
            var interfaceBody = TsTypePrinter.TabScope(string.Join("\n", tsContent));

            return $"interface {interfaceName}{extendsExpression} {{ {interfaceBody}}}";
        }

        private static string[] ConvertFields(Type dotType)
        {
            var fields = dotType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            var fieldExpressions = fields.Select(f =>
            {
                var tsField = TsField.Convert(f);
                return tsField;
            }).ToArray();

            return fieldExpressions;
        }

        private static string[] ConvertMethods(Type dotType)
        {
            var methods = dotType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            var methodExpressions = methods.Where(m => !m.IsGenericMethod).Select(m =>
            {
                var tsMethod = TsMethod.Convert(m);
                return tsMethod;
            }).ToArray();

            return methodExpressions;
        }
    }
}