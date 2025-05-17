using System;

namespace XGames.TsTranslator
{
    public static class TsBuilder
    {
        public static bool TryBuild(Type dotType, out string result)
        {
            result = "";

            if (!dotType.IsPublic || dotType.IsGenericType)
            {
                return false;
            }

            string tsBody;
            if (dotType.IsEnum)
            {
                var tsEnum = TsEnum.Convert(dotType);
                tsBody = tsEnum;
            }
            else if (dotType.IsClass || dotType.IsValueType)
            {
                var tsClass = TsClass.Convert(dotType);
                tsBody = tsClass;
            }
            else if (dotType.IsInterface)
            {
                var tsInterface = TsInterface.Convert(dotType);
                tsBody = tsInterface;
            }
            else
            {
                return false;
            }

            var nameSpace = dotType.Namespace;
            if (string.IsNullOrWhiteSpace(nameSpace))
            {
                result = $"declare namespace CS {{ {TsTypePrinter.TabScope(tsBody)}}}";
            }
            else
            {
                result = $"declare namespace CS.{nameSpace} {{ {TsTypePrinter.TabScope(tsBody)}}}";
            }

            return true;
        }
    }
}