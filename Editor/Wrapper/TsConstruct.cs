using System.Linq;
using System.Reflection;

namespace XGames.TsTranslator
{
    public class TsConstruct
    {
        public static string Convert(ConstructorInfo constructorInfo)
        {
            var @params = ConvertParams(constructorInfo);
            var paramsContent = string.Join(", ", @params);

            var staticKey = constructorInfo.IsStatic ? "static " : "";
            var staticParam = constructorInfo.IsStatic ? "this: void, " : "";

            return $"{staticKey}constructor({staticParam}{paramsContent});";
        }
        
        private static string[] ConvertParams(ConstructorInfo constructorInfo)
        {
            var parameters = constructorInfo.GetParameters();
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