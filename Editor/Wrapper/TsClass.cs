using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XGames.TsTranslator
{
    public static class TsClass
    {
        public static string Convert(Type dotType)
        {
            var className = dotType.Name;

            var extendsName = string.Empty;
            if (dotType.BaseType is { IsValueType: false })
            {
                extendsName = TsType.Convert(dotType.BaseType);
            }

            var extendsExpression = !string.IsNullOrEmpty(extendsName) && extendsName != "any"
                ? $" extends {extendsName}"
                : "";

            var implements = ConvertImps(dotType);
            var implementExpression = implements.Length > 0 ? $" implements {string.Join(", ", implements)}" : "";

            var properties = ConvertProps(dotType);
            var fields = ConvertFields(dotType);
            var constructs = ConvertConstruct(dotType);
            var methods = ConvertMethods(dotType);
            var tsContent = properties.Concat(fields).Concat(constructs).Concat(methods);
            var classBody = TsTypePrinter.TabScope(string.Join("\n", tsContent));

            var tsClass = $"class {className}{extendsExpression}{implementExpression} {{ {classBody}}}";
            return tsClass;
        }

        private static string[] ConvertImps(Type dotType)
        {
            var interfaces = dotType.GetInterfaces();
            var implements = interfaces.Where(type =>
            {
                var typeName = type.ToString();
                var exclude = typeName != "System.Runtime.InteropServices._Attribute";
                return exclude;
            }).Select(type =>
            {
                var tsType = TsType.Convert(type);
                return tsType;
            }).Where(tsType =>
            {
                var include = !string.IsNullOrEmpty(tsType) && tsType != TsType.ANY;
                return include;
            }).ToArray();

            return implements;
        }

        private static string[] ConvertProps(Type dotType)
        {
            var allProps = new HashSet<string>();

            var properties = dotType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            var propExpressions = properties.Where(prop =>
            {
                if (!allProps.Contains(prop.Name))
                {
                    allProps.Add(prop.Name);
                    return true;
                }

                return false;
            }).Select(prop =>
            {
                var tsProp = TsProperty.Convert(prop);
                return tsProp;
            }).ToArray();

            allProps.Clear();

            return propExpressions;
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
            var methodExpressions = methods.Select(m =>
            {
                var tsMethod = TsMethod.Convert(m);
                return tsMethod;
            }).ToArray();

            return methodExpressions;
        }

        private static string[] ConvertConstruct(Type dotType)
        {
            var constructs = dotType.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            var constructExpressions = constructs.Select(m =>
            {
                var tsConstruct = TsConstruct.Convert(m);
                return tsConstruct;
            }).ToArray();

            return constructExpressions;
        }
    }
}