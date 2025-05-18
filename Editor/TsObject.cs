using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XGames.TsTranslator
{
    public static class TsObject
    {
        public static void ConvertClass(Type dotType, in StringBuilder sb)
        {
            var className = dotType.Name;
            var customConstructor = $"/** @customConstructor {className} */";
            sb.AppendLine(customConstructor);
            sb.Append($"class {className}");
            ConvertExtends(dotType, in sb);
            ConvertImps(dotType, in sb);

            sb.AppendLine("{");
            ConvertProps(dotType, in sb);
            ConvertFields(dotType, in sb);
            ConvertConstruct(dotType, in sb);
            ConvertMethods(dotType, in sb);
            sb.AppendLine("}");
        }

        public static void ConvertInterface(Type dotType, in StringBuilder sb)
        {
            var interfaceName = dotType.Name;
            sb.Append($"interface {interfaceName}");
            ConvertExtends(dotType, in sb);
            
            sb.AppendLine("{");
            ConvertFields(dotType, in sb);
            ConvertMethods(dotType, in sb);
            sb.AppendLine("}");
        }

        private static void ConvertExtends(Type dotType, in StringBuilder sb)
        {
            if (dotType.BaseType is { IsValueType: false })
            {
                var extendsClass = TsType.Convert(dotType.BaseType);
                if (extendsClass != TsType.Any)
                {
                    sb.Append($" extends {extendsClass}");
                }
            }
        }

        private static void ConvertImps(Type dotType, in StringBuilder sb)
        {
            var interfaces = dotType.GetInterfaces();
            var isEmpty = true;
            foreach (var i in interfaces)
            {
                var interfaceType = TsType.Convert(i);
                if (!string.IsNullOrEmpty(interfaceType) && interfaceType != TsType.Any)
                {
                    if (isEmpty)
                    {
                        isEmpty = false;
                        sb.Append(" implements ");
                        sb.Append($"{interfaceType}");
                    }
                    else
                    {
                        sb.Append($", {interfaceType}");
                    }
                }
            }
        }

        private static readonly HashSet<string> AllProps = new();

        private static void ConvertProps(Type dotType, in StringBuilder sb)
        {
            AllProps.Clear();
            var properties = dotType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (!AllProps.Add(prop.Name)) continue; // 屏蔽不同参数，重复属性名
                ConvertOneProperty(prop, out var getProp, out var setProp);
                if (getProp != null)
                {
                    sb.AppendLine(getProp);
                }

                if (setProp != null)
                {
                    sb.AppendLine(setProp);
                }
            }
        }

        private static void ConvertOneProperty(PropertyInfo propertyInfo, out string getProp, out string setProp)
        {
            var propName = propertyInfo.Name;
            var propType = TsType.Convert(propertyInfo.PropertyType);

            if (propertyInfo.CanRead)
            {
                var staticKey = propertyInfo.GetMethod.IsStatic ? "static " : "";
                getProp = $"{staticKey}get {propName}(): {propType};";
            }
            else
            {
                getProp = null;
            }

            if (propertyInfo.CanWrite)
            {
                var staticKey = propertyInfo.SetMethod.IsStatic ? "static " : "";
                setProp = $"{staticKey}set {propName}(value: {propType});";
            }
            else
            {
                setProp = null;
            }
        }

        private static void ConvertFields(Type dotType, in StringBuilder sb)
        {
            var fields = dotType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            foreach (var f in fields)
            {
                var fieldTsType = TsType.Convert(f.FieldType);
                var fieldName = f.Name;
                if (f.IsStatic)
                {
                    sb.Append("static ");
                }

                var fieldExpression = $"{fieldName}: {fieldTsType};";
                sb.AppendLine(fieldExpression);
            }
        }

        private static void ConvertMethods(Type dotType, in StringBuilder sb)
        {
            var methods = dotType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            foreach (var m in methods)
            {
                if (m.IsGenericMethod) continue;
                ConvertOneMethod(m, sb);
            }
        }

        private static void ConvertConstruct(Type dotType, in StringBuilder sb)
        {
            var constructs = dotType.GetConstructors(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            foreach (var m in constructs)
            {
                ConvertOneMethod(m, sb, true);
            }
        }

        private static void ConvertOneMethod(MethodBase methodBase, in StringBuilder sb, bool isConstructor = false)
        {
            var methodName = methodBase.Name;
            var isStatic = methodBase.IsStatic;
            if (isStatic) sb.Append("static ");
            sb.Append(isConstructor ? "constructor" : methodName);
            if (methodBase.IsGenericMethod)
            {
                var genericArguments = methodBase.GetGenericArguments().Select(TsType.Convert);
                sb.Append($"<{string.Join(", ", genericArguments)}>");
            }

            sb.Append("(");
            var isParamEmpty = true;
            if (isStatic)
            {
                sb.Append("this: void");
                isParamEmpty = false;
            }

            var outReturnType = new List<string>();
            var parameters = methodBase.GetParameters();
            foreach (var parameterInfo in parameters)
            {
                var paramType = TsType.Convert(parameterInfo.ParameterType);
                if (parameterInfo.IsOut)
                {
                    outReturnType.Add(paramType);
                }
                else
                {
                    var paramName = parameterInfo.Name;
                    var paramExpression = $"{paramName}: {paramType}";
                    if (isParamEmpty)
                    {
                        isParamEmpty = false;
                        sb.Append(paramExpression);
                    }
                    else
                    {
                        sb.Append($", {paramExpression}");
                    }
                }
            }

            var methodInfo = methodBase as MethodInfo;
            if (methodInfo == null)
            {
                sb.AppendLine(");");
                return;
            }

            sb.Append("): ");
            var returnType = TsType.Convert(methodInfo.ReturnType);
            if (outReturnType.Count > 0)
            {
                if (methodInfo.ReturnType != typeof(void))
                {
                    outReturnType.Insert(0, returnType);
                }

                returnType = $"LuaMultiReturn<[{string.Join(", ", outReturnType)}]>";
            }

            sb.AppendLine($"{returnType};");
        }
    }
}