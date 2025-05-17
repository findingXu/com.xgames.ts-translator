using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XGames.TsTranslator
{
    public static class TsTypePrinter
    {
        public static Type[] LuaCallCSharp;

        public static void GenCode(IEnumerable<Type> luaCallCSharp, string outFile)
        {
            var allTypes = new List<string>();
            LuaCallCSharp = luaCallCSharp.ToArray();
            foreach (var type in LuaCallCSharp)
            {
                if (TsBuilder.TryBuild(type, out var result))
                {
                    allTypes.Add(result);
                }
            }
            
            var tsTypeFilePath = Path.Combine(Application.dataPath, outFile);
            using (var file = new FileStream(tsTypeFilePath, FileMode.OpenOrCreate))
            {
                var bytes = Encoding.UTF8.GetBytes(string.Join("\n", allTypes));
                file.Write(bytes);
            }

            Debug.Log($"ts type code generate completed at: {tsTypeFilePath}");
        }
        
        public static string TabScope(string scope, bool withEndl = true)
        {
            var tabScope = scope.Replace("\n", $"\n\t");
            if (withEndl)
            {
                tabScope = $"{tabScope}\n";
            }

            return $"\n\t{tabScope}";
        }
    }
}