using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ucmd.BuildPlayer
{
    public class StaticCall
    {

        public static readonly Dictionary<string, string> ArgMap = new Dictionary<string, string>();

        public static void Run()
        {
            PrepareArgs();
            var isSuc = ArgMap.TryGetValue("_targetPlatform", out var plat);
            if (!isSuc) return;
            switch (plat)
            {
                case "android":
                    new PerformBuildAndroid().Run();
                    break;
                case "ios":
                    new PerformBuildIOS().Run();
                    break;
                default:
                    Debug.LogError($"Platform of <{plat}> is not support yet!");
                    break;
            }

        }

        private static void PrepareArgs()
        {
            var cmdArgs = Environment.GetCommandLineArgs();
            var disVal = "";
            foreach (var v in cmdArgs)
            {
                if (v.StartsWith("-") && v.Split(':').Length > 1)
                {
                    var spIndex = v.IndexOf(":",  StringComparison.Ordinal);
                    var k = v.Substring(1, spIndex - 1);
                    Debug.Log($"k={k}, spindex={spIndex}111111111");
                    var val = v.Substring(spIndex + 1); 
                    Debug.Log($"k={k} , v={val}   111111111");
                    ArgMap[k] = val;
                    disVal += $"{k}: {val}\n";
                }
            }
            Debug.Log($@"
***********************************
Option params in command:
{disVal.Substring(0, disVal.Length - 1)}
***********************************
");
        }
    }
}