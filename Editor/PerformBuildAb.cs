using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ucmd.BuildPlayer
{
    public class PerformBuildAb : BuildBase
    {

        private static BuildTarget _buildTarget = BuildTarget.NoTarget;


        private static string _abMap = "";
        
        internal PerformBuildAb(){
            _abMap = StaticCall.ArgMap.ContainsKey("abMap") ? StaticCall.ArgMap["abMap"] : string.Empty;
        }
        

        private static List<string> GetPathAssets(string path)
        {
            var al = AssetDatabase.GetAllAssetPaths();
            var rl = new List<string>();
            foreach (var p in al)
            {
                if (p.StartsWith(path) && p != path)
                {
                    Debug.Log($"Find the asset path is {p}");
                    if (p.EndsWith(".cs") ||
                         p.EndsWith(".unity") ||
                          p.Contains("squishy")) continue;
                    if (IsSepLuaScript(p)) continue;
                    if (!Path.HasExtension(p)) continue;
                    rl.Add(p);
                }
            }

            return rl;
        }

        private static bool IsSepLuaScript(string fName)
        {
            return fName.Contains(".lua") &&
                   !fName.Contains("luapack.lua");
            
        }

        private static (string, BuildTarget) GetOsExStr()
        {
            var tarp = EditorUserBuildSettings.activeBuildTarget;
            switch (tarp)
            {
                case BuildTarget.Android:
                    return ("and", BuildTarget.Android);
                case BuildTarget.iOS:
                    return ("ios", BuildTarget.iOS);
                default:
                    Debug.LogWarning("Activity buidl target {} not support ab build yet!do nothing!");
                    return ("unknown", BuildTarget.NoTarget);
            }
        }

        // private static string SquishLuaPack(string rootDir)
        // {
        //     var outF = "";
        //     const string luaF = "luapack.lua.txt";
        //     Debug.Log("Check there is lua scripts need to pack....");
        //     var srcPath =  Path.Combine(Application.dataPath,"../../../../", rootDir, "scripts");
        //     if (Directory.Exists(srcPath))
        //     {
        //         var tire = $"{Application.dataPath}/../../../tools/luapack";
        //         var cmd = $"{tire}/squish {srcPath}";

        //         var ret = BuildHelper.RunCmd(Path.Combine(tire, "lua"), cmd);
        //         if (!ret[0].Contains("OK!"))
        //         {
        //             throw new Exception($"Run squish command raise error!{ret[0]}");
        //         }

        //         if (!File.Exists(luaF))
        //         {
        //             throw new Exception($"Not found the squish build file {luaF}");
        //         }

        //         outF = Path.Combine(Application.dataPath,"../../../../", rootDir, luaF);
        //         if (File.Exists(outF))
        //         {
        //             File.Delete(outF);
        //         }

        //         File.Move(luaF, outF);
        //         Debug.Log("Lua script pack suc!");
        //     }

        //     return outF;
        // }




        /// <summary>
        /// 从外部调用 构建ab包
        /// </summary>
        public void Run()
        {
            var dir = BuildHelper.CheckBuildPath(OutputPath);
            if (_abMap == string.Empty)
            {
                Debug.LogWarning("No <abMap> params set , so build noting!");
                return;
            }
            
            var (ex, tar) = GetOsExStr();
            Debug.Log($"Found ab build params-->> target={tar}, ex={ex}");
            var ls = _abMap.Split('|');
            Debug.Log($"{ls}");
            foreach (var p in ls)
            {
                var vl = p.Split('=');
                Debug.Log($"out value is {string.Join(",", vl)}");
                var assetPath = vl[1];
                var name = vl[0];
                var pl = GetPathAssets(assetPath);
                foreach (var f in pl)
                {
                    var ai = AssetImporter.GetAtPath(f);
                    ai.assetBundleName = name;
                }
                var cur = $"{DateTime.Now:yyyyMMddhhmmss}";
                Debug.Log($"build file target is ->bundle_{name}{ex}_{cur}.ab");
                var ab = new AssetBundleBuild()
                {
                    assetBundleName = $"bundle_{name}{ex}_{cur}.ab",
                    assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name)
                };
                BuildPipeline.BuildAssetBundles(dir, new[] {ab}, BuildAssetBundleOptions.None, tar);
            }
            ExecuteHook(HookType.Finish);
        }

    }
}