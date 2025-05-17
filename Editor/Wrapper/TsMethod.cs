using System.Linq;
using System.Reflection;

namespace XGames.TsTranslator
{
    public static class TsMethod
    {
        public static string Convert(MethodInfo methodInfo)
        {
            var methodName = methodInfo.Name;

            if (methodInfo.IsGenericMethod)
            {
                var genericArguments = methodInfo.GetGenericArguments().Select(g =>
                {
                    var genType = TsType.Convert(g);
                    return genType;
                }).ToArray();
                methodName += $"<{string.Join(", ", genericArguments)}>";
            }

            var returnType = TsType.Convert(methodInfo.ReturnType);
            var @params = ConvertParams(methodInfo);
            var paramsContent = string.Join(", ", @params);

            var staticKey = methodInfo.IsStatic ? "static " : "";
            var staticParam = methodInfo.IsStatic ? "this: void, " : "";

            return $"{staticKey}{methodName}({staticParam}{paramsContent}): {returnType};";
        }

        private static string[] ConvertParams(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var paramExpressions = parameters.Select(parameterInfo =>
            {
                var paramName = parameterInfo.Name;
                var paramType = TsType.Convert(parameterInfo.ParameterType);
                return $"{paramName}: {paramType}";
            }).ToArray();

            return paramExpressions;
        }
    }
}