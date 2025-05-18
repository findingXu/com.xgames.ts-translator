using System.Collections.Generic;
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

            var (@params, outReturnType) = ConvertParams(methodInfo);
            var paramsContent = string.Join(", ", @params);

            var staticKey = methodInfo.IsStatic ? "static " : "";
            var staticParam = methodInfo.IsStatic ? "this: void, " : "";
            var returnType = TsType.Convert(methodInfo.ReturnType);
            if (outReturnType.Count > 0)
            {
                if (methodInfo.ReturnType != typeof(void))
                {
                    outReturnType.Insert( 0, returnType);
                }

                returnType = $"LuaMultiReturn<[{string.Join(", ", outReturnType)}]>";
            }

            return $"{staticKey}{methodName}({staticParam}{paramsContent}): {returnType};";
        }

        private static (List<string>, List<string>) ConvertParams(MethodInfo methodInfo)
        {
            var paramExpressions = new List<string>();
            var outExpressions = new List<string>();
            var parameters = methodInfo.GetParameters();
            foreach (var parameterInfo in parameters)
            {
                var paramType = TsType.Convert(parameterInfo.ParameterType);
                if (parameterInfo.IsOut)
                {
                    outExpressions.Add(paramType);
                }
                else
                {
                    var paramName = parameterInfo.Name;
                    paramExpressions.Add($"{paramName}: {paramType}");
                }
            }

            return (paramExpressions, outExpressions);
        }
    }
}