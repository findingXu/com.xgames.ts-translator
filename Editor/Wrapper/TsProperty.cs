using System.Reflection;
using System.Text;

namespace XGames.TsTranslator
{
    public static class TsProperty
    {
        public static string Convert(PropertyInfo propertyInfo)
        {
            var propName = propertyInfo.Name;
            var propType = TsType.Convert(propertyInfo.PropertyType);

            var sbProperty = new StringBuilder();
            if (propertyInfo.CanRead)
            {
                var staticKey = propertyInfo.GetMethod.IsStatic ? "static " : "";
                sbProperty.Append($"{staticKey}get {propName}(): {propType};");
            }

            if (propertyInfo.CanWrite)
            {
                if (propertyInfo.CanRead)
                {
                    sbProperty.Append("\n");
                }
                
                var staticKey = propertyInfo.SetMethod.IsStatic ? "static " : "";
                sbProperty.Append($"{staticKey}set {propName}(value: {propType});");
            }

            return sbProperty.ToString();
        }
    }
}