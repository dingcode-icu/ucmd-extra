using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ucmd.BuildPlayer
{
    public class PerformBuildAb : BuildBase
    {
        private static string _relPath;
        private static readonly string AbBuildPath = Application.dataPath + "/../../../build/AbBuild";

        private static List<string> GetPathAssets(string path)
        {
            var al = AssetDatabase.GetAllAssetPaths();
            var rl = new List<string>();
            foreach (var p in al)
            {
                if (p.StartsWith(path) && p != path)
                {
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

        private static (string, BuildTarget) GetOsExStr(string tarp)
        {
            switch (tarp)
            {
                case "android":
                    return ("and", BuildTarget.Android);
                case "ios":
                    return ("ios", BuildTarget.iOS);
                default:
                    return ("", BuildTarget.StandaloneOSX);
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
        public static void Build()
        {
            //检查参数
            CheckOptionArgs();
            CheckRequireArgs();

            var dir = BuildHelper.CheckBuildPath(AbBuildPath);
            Debug.Log(_relPath);
            if (_relPath == string.Empty)
            {
                Debug.Log("Not found relative path to execute build asset bundle. ");
                Environment.Exit(0);
            }

            var ls = _relPath.Split('|');
            Debug.Log($"{ls}");
            foreach (var p in ls)
            {
                var vl = p.Split('=');
                Debug.Log($"out value is {string.Join(",", vl)}");
                var name = vl[1];
                var assetPath = vl[0];
                var pl = GetPathAssets(assetPath);
                foreach (var f in pl)
                {

                    var ai = AssetImporter.GetAtPath(f);

                    ai.assetBundleName = name;
                }

                var (ex, tar) = GetOsExStr(TargetPlatform);
                var cur = $"{DateTime.Now:yyyyMMddhhmmss}";
                Debug.Log($"bundle_{name}{ex}_{cur}.ab---->>>>11");
                var ab = new AssetBundleBuild()
                {
                    assetBundleName = $"bundle_{name}{ex}_{cur}.ab",
                    assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(name)
                };
                BuildPipeline.BuildAssetBundles(dir, new[] {ab}, BuildAssetBundleOptions.None, tar);
            }

            ExecuteHook(HookType.Finish);
        }

        protected new static void CheckOptionArgs()
        {
            BuildBase.CheckOptionArgs();
            var args = Environment.GetCommandLineArgs();
            foreach (var s in args)
            {
                //检查要打包的相对路径列表
                if (s.Contains("-abMap:"))
                {
                    var o = s.Split(':')[1];
                    _relPath = o.Substring(0, o.Length - 1);
                    ;
                    Debug.Log($"rel value is {_relPath}");
                }

                //检查目标平台参数
                if (SupportPlat.Contains(s))
                {
                    TargetPlatform = s;
                }
            }
        }
    }
}