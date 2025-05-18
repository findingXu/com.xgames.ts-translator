using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XGames.TsTranslator
{
    public static class TypeTranslator
    {
        public static HashSet<Type> LuaCallCSharp;

        public static void GenCode(IEnumerable<Type> luaCallCSharp, string outFile)
        {
            LuaCallCSharp = luaCallCSharp.ToHashSet();
            
            var sb = new StringBuilder();
            foreach (var type in LuaCallCSharp)
            {
                WriteType(type, in sb);
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            var tsTypeFilePath = Path.Combine(Application.dataPath, outFile);
            using (var file = new FileStream(tsTypeFilePath, FileMode.OpenOrCreate))
            {
                file.SetLength(0);
                file.Write(bytes);
            }

            Debug.Log($"ts type code generate completed at: {tsTypeFilePath}");
        }
        
        private static void WriteType(Type dotType, in StringBuilder sb)
        {
            if (!dotType.IsPublic || dotType.IsGenericType) return;

            var nameSpace = string.IsNullOrWhiteSpace(dotType.Namespace) ? "CS" : $"CS.{dotType.Namespace}";
            sb.AppendLine($"declare namespace {nameSpace} {{");
            if (dotType.IsEnum)
            {
                TsEnum.Convert(dotType, in sb);
            }
            else if (dotType.IsClass || dotType.IsValueType)
            {
                TsObject.ConvertClass(dotType, in sb);
            }
            else if (dotType.IsInterface)
            {
                TsObject.ConvertInterface(dotType, in sb);
            }
            sb.AppendLine("}");
        }
    }
}